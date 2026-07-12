using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.Sales.DTOs;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Entities.Sales;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Commands.CreateSale;

public record CreateSaleCommand(
    Guid? CustomerId,
    List<CreateSaleItemRequest> Items,
    string PaymentMethod,
    string Channel,
    string Currency,
    string? Notes,
    // Importación histórica: no valida ni descuenta stock, y usa la fecha provista.
    bool IsHistorical = false,
    DateTime? SaleDate = null,
    string? CustomerName = null
) : IRequest<Guid>;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    private static readonly string[] ValidChannels = ["InStore", "Phone", "WhatsApp", "Other"];
    private static readonly string[] ValidPaymentMethods = ["Cash", "Card", "Transfer", "QR", "Mixed"];

    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("La venta debe tener al menos un ítem.");
        RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("El método de pago es requerido.");
        RuleFor(x => x.Channel)
            .Must(c => ValidChannels.Contains(c, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Canal inválido. Opciones: {string.Join(", ", ValidChannels)}");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });

        // El producto/variante real solo se exige en ventas normales (no en importación histórica).
        When(x => !x.IsHistorical, () =>
        {
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.VariantId).NotEmpty();
            });
        });
    }
}

public class CreateSaleCommandHandler(
    ISaleRepository saleRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateSaleCommand, Guid>
{
    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken ct)
    {
        // ── Importación histórica: registra la venta con su fecha, sin tocar stock ──
        if (request.IsHistorical)
            return await HandleHistorical(request, ct);

        // Validar stock e ítems
        var resolvedItems = new List<(CreateSaleItemRequest Req, string ProductName, string Sku, decimal? Cost)>();
        foreach (var itemReq in request.Items)
        {
            var product = await productRepository.GetWithVariantsAsync(itemReq.ProductId, ct)
                ?? throw new NotFoundException($"Producto '{itemReq.ProductId}' no encontrado.");

            var variant = product.Variants.FirstOrDefault(v => v.Id == itemReq.VariantId)
                ?? throw new NotFoundException($"Variante '{itemReq.VariantId}' no encontrada.");

            if (!variant.IsActive)
                throw new ConflictException($"La variante '{variant.Sku}' no está activa.");

            if (variant.StockQuantity < itemReq.Quantity)
                throw new ConflictException($"Stock insuficiente para '{variant.Sku}'. Disponible: {variant.StockQuantity}.");

            resolvedItems.Add((itemReq, product.Name, variant.Sku, variant.Cost?.Amount));
        }

        var saleNumber = await saleRepository.GenerateSaleNumberAsync(ct);
        var channel = Enum.Parse<SaleChannel>(request.Channel, ignoreCase: true);
        var userId = currentUser.UserId ?? Guid.Empty;

        var sale = Sale.Create(saleNumber, userId, request.PaymentMethod, channel, request.Currency, request.CustomerId, request.Notes);
        unitOfWork.Add(sale);

        // Agregar ítems y descontar stock
        foreach (var (req, productName, sku, cost) in resolvedItems)
        {
            var unitPrice = Money.Of(req.UnitPrice, request.Currency);
            var discount = Money.Of(req.Discount, request.Currency);
            var unitCost = cost.HasValue ? Money.Of(cost.Value, request.Currency) : null;
            sale.AddItem(req.ProductId, req.VariantId, productName, sku, req.Quantity, unitPrice, discount, unitCost);

            // Descontar stock inmediatamente (venta directa)
            var product = await productRepository.GetWithVariantsAsync(req.ProductId, ct);
            var variant = product!.Variants.First(v => v.Id == req.VariantId);
            var stockBefore = variant.StockQuantity;
            variant.DecrementStock(req.Quantity);

            var movement = StockMovement.Create(
                req.ProductId, req.VariantId, MovementType.SaleOut,
                req.Quantity, stockBefore, variant.StockQuantity,
                referenceType: "Sale",
                reason: $"Venta {saleNumber}",
                createdByUserId: userId);
            unitOfWork.Add(movement);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return sale.Id;
    }

    // ── Importación histórica ───────────────────────────────────────────────────
    private async Task<Guid> HandleHistorical(CreateSaleCommand request, CancellationToken ct)
    {
        var saleNumber = await saleRepository.GenerateSaleNumberAsync(ct);
        var channel = Enum.Parse<SaleChannel>(request.Channel, ignoreCase: true);
        var userId = currentUser.UserId ?? Guid.Empty;

        // El nombre del comprador va en las notas (no hay cliente real asociado).
        var notes = request.Notes;
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
            notes = string.IsNullOrWhiteSpace(notes)
                ? $"Cliente: {request.CustomerName}"
                : $"Cliente: {request.CustomerName} · {notes}";

        var sale = Sale.Create(saleNumber, userId, request.PaymentMethod, channel,
            request.Currency, customerId: null, notes: notes, createdAt: request.SaleDate);
        unitOfWork.Add(sale);

        foreach (var item in request.Items)
        {
            var unitPrice = Money.Of(item.UnitPrice, request.Currency);
            var discount = Money.Of(item.Discount, request.Currency);
            var unitCost = item.UnitCost.HasValue ? Money.Of(item.UnitCost.Value, request.Currency) : null;
            sale.AddItem(
                item.ProductId, item.VariantId,
                item.ProductName ?? "Producto", item.Sku ?? "-",
                item.Quantity, unitPrice, discount, unitCost);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return sale.Id;
    }
}
