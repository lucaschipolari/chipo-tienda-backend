using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.Sales.DTOs;
using ChipoBackend.Application.Features.Settings;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Entities.Sales;
using System.Text.RegularExpressions;
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
    IUnitOfWork unitOfWork,
    IAppSettingRepository appSettings
) : IRequestHandler<CreateSaleCommand, Guid>
{
    public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken ct)
    {
        // ── Importación histórica: registra la venta con su fecha, sin tocar stock ──
        if (request.IsHistorical)
            return await HandleHistorical(request, ct);

        // Validar stock e ítems
        var resolved = new List<(CreateSaleItemRequest Req, Product Product, ProductVariant Variant, int MlPerUnit)>();
        foreach (var itemReq in request.Items)
        {
            var product = await productRepository.GetWithVariantsAsync(itemReq.ProductId, ct)
                ?? throw new NotFoundException($"Producto '{itemReq.ProductId}' no encontrado.");

            var variant = product.Variants.FirstOrDefault(v => v.Id == itemReq.VariantId)
                ?? throw new NotFoundException($"Variante '{itemReq.VariantId}' no encontrada.");

            if (!variant.IsActive)
                throw new ConflictException($"La variante '{variant.Sku}' no está activa.");

            if (product.IsDecant)
            {
                // Decant: el stock se mide en ml (pool compartido del frasco)
                var mlPerUnit = ParseMl(variant.Attributes);
                var mlNeeded = mlPerUnit * itemReq.Quantity;
                if (mlNeeded <= 0)
                    throw new ConflictException($"La variante '{variant.Sku}' no tiene tamaño en ml definido.");
                if (product.StockMl < mlNeeded)
                    throw new ConflictException($"Stock insuficiente de '{product.Name}'. Disponible: {product.StockMl} ml, necesita: {mlNeeded} ml.");
                resolved.Add((itemReq, product, variant, mlPerUnit));
            }
            else
            {
                if (variant.StockQuantity < itemReq.Quantity)
                    throw new ConflictException($"Stock insuficiente para '{variant.Sku}'. Disponible: {variant.StockQuantity}.");
                resolved.Add((itemReq, product, variant, 0));
            }
        }

        var saleNumber = await saleRepository.GenerateSaleNumberAsync(ct);
        var channel = Enum.Parse<SaleChannel>(request.Channel, ignoreCase: true);
        var userId = currentUser.UserId ?? Guid.Empty;

        // Costo de frasquitos por tamaño (global) — se suma al costo de cada decant
        var vialCosts = VialCostSettings.Parse((await appSettings.GetAsync(VialCostSettings.Key, ct))?.Value);

        var sale = Sale.Create(saleNumber, userId, request.PaymentMethod, channel, request.Currency, request.CustomerId, request.Notes, customerName: request.CustomerName);
        unitOfWork.Add(sale);

        // Agregar ítems y descontar stock
        foreach (var (req, product, variant, mlPerUnit) in resolved)
        {
            var unitPrice = Money.Of(req.UnitPrice, request.Currency);
            var discount = Money.Of(req.Discount, request.Currency);

            Money? unitCost;
            if (product.IsDecant)
            {
                // Costo = líquido (ml × costo por ml del frasco) + frasquito (según tamaño)
                var costPerMl = product.CostPerMl ?? 0m;
                var vial = vialCosts.GetValueOrDefault(mlPerUnit, 0m);
                unitCost = Money.Of(mlPerUnit * costPerMl + vial, request.Currency);
            }
            else
            {
                unitCost = variant.Cost is { } c ? Money.Of(c.Amount, request.Currency) : null;
            }

            sale.AddItem(req.ProductId, req.VariantId, product.Name, variant.Sku, req.Quantity, unitPrice, discount, unitCost);

            if (product.IsDecant)
            {
                var mlNeeded = mlPerUnit * req.Quantity;
                var beforeMl = product.StockMl;
                product.DecrementMl(mlNeeded);
                var movement = StockMovement.Create(
                    req.ProductId, req.VariantId, MovementType.SaleOut,
                    mlNeeded, beforeMl, product.StockMl,
                    referenceType: "Sale", reason: $"Venta {saleNumber} ({mlNeeded} ml)",
                    createdByUserId: userId);
                unitOfWork.Add(movement);
            }
            else
            {
                var stockBefore = variant.StockQuantity;
                variant.DecrementStock(req.Quantity);
                var movement = StockMovement.Create(
                    req.ProductId, req.VariantId, MovementType.SaleOut,
                    req.Quantity, stockBefore, variant.StockQuantity,
                    referenceType: "Sale", reason: $"Venta {saleNumber}",
                    createdByUserId: userId);
                unitOfWork.Add(movement);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return sale.Id;
    }

    // Extrae los ml de una variante decant desde su atributo "Tamaño" (ej. "5ml" -> 5).
    private static int ParseMl(Dictionary<string, string> attributes)
    {
        if (attributes == null) return 0;
        foreach (var v in attributes.Values)
        {
            var m = Regex.Match(v ?? "", @"(\d+)\s*ml", RegexOptions.IgnoreCase);
            if (m.Success) return int.Parse(m.Groups[1].Value);
        }
        return 0;
    }

    // ── Importación histórica ───────────────────────────────────────────────────
    private async Task<Guid> HandleHistorical(CreateSaleCommand request, CancellationToken ct)
    {
        var saleNumber = await saleRepository.GenerateSaleNumberAsync(ct);
        var channel = Enum.Parse<SaleChannel>(request.Channel, ignoreCase: true);
        var userId = currentUser.UserId ?? Guid.Empty;

        var sale = Sale.Create(saleNumber, userId, request.PaymentMethod, channel,
            request.Currency, customerId: null, notes: request.Notes, createdAt: request.SaleDate,
            customerName: request.CustomerName);
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
