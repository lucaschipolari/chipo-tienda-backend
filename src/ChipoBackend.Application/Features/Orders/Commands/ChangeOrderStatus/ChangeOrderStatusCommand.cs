using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.Settings;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Entities.Orders;
using ChipoBackend.Domain.Entities.Sales;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Orders.Commands.ChangeOrderStatus;

public record ChangeOrderStatusCommand(
    Guid OrderId,
    string NewStatus,
    string? Note = null
) : IRequest;

public class ChangeOrderStatusCommandValidator : AbstractValidator<ChangeOrderStatusCommand>
{
    private static readonly string[] ValidStatuses = ["Confirmed", "Paid", "Processing", "Shipped", "Delivered", "Cancelled", "Refunded"];

    public ChangeOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Estado inválido. Valores permitidos: {string.Join(", ", ValidStatuses)}");
    }
}

public class ChangeOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IStockMovementRepository stockMovementRepository,
    ISaleRepository saleRepository,
    IAppSettingRepository appSettings,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<ChangeOrderStatusCommand>
{
    public async Task Handle(ChangeOrderStatusCommand request, CancellationToken ct)
    {
        var order = await orderRepository.GetForStatusUpdateAsync(request.OrderId, ct)
            ?? throw new NotFoundException($"Pedido '{request.OrderId}' no encontrado.");

        var previousStatus = order.Status;
        var userId = currentUser.UserId;

        switch (request.NewStatus.ToLowerInvariant())
        {
            case "confirmed":
                order.Confirm(userId);
                // Descontar stock al confirmar
                await DeductStockForOrderAsync(order, ct);
                break;

            case "paid":
                order.MarkAsPaid(userId);
                // Si aún no se descontó stock (no pasó por Confirmado), descontarlo ahora.
                if (previousStatus < OrderStatus.Confirmed)
                    await DeductStockForOrderAsync(order, ct);
                // Convertir el pedido en una venta (para que sume a la ganancia).
                // Solo la primera vez que pasa a Pagado.
                if (previousStatus != OrderStatus.Paid)
                    await CreateSaleFromOrderAsync(order, ct);
                break;

            case "processing":
                order.StartProcessing(userId);
                break;

            case "shipped":
                order.Ship(userId, request.Note);
                break;

            case "delivered":
                order.Deliver(userId);
                break;

            case "cancelled":
                if (string.IsNullOrWhiteSpace(request.Note))
                    throw new ConflictException("El motivo de cancelación es requerido.");
                order.Cancel(request.Note!, userId);
                // Restaurar stock si ya se había descontado (estaba Confirmed o más)
                if (previousStatus >= OrderStatus.Confirmed)
                    await RestoreStockForOrderAsync(order, ct);
                break;

            case "refunded":
                // Para refund solo actualizamos estado
                if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Paid)
                    throw new ConflictException("Solo se puede reembolsar un pedido entregado o pagado.");
                // Usamos el evento de cancelación para registrar el cambio
                order.Cancel("Pedido reembolsado", userId);
                break;

            default:
                throw new ConflictException($"Estado '{request.NewStatus}' no reconocido.");
        }

        // Registrar explícitamente las entradas de historial como Added.
        // GetForStatusUpdateAsync no carga StatusHistory, por lo que EF no tiene
        // snapshot de esa colección y detecta las nuevas entradas como Modified
        // (adjuntadas) en lugar de Added. La llamada explícita a Add<T> fuerza
        // el estado correcto y EF genera INSERT en lugar de UPDATE.
        foreach (var h in order.StatusHistory)
            unitOfWork.Add(h);

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task DeductStockForOrderAsync(Order order, CancellationToken ct)
    {
        foreach (var item in order.Items)
        {
            var product = await productRepository.GetWithVariantsAsync(item.ProductId, ct);
            if (product == null) continue;

            var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId);
            if (variant == null) continue;

            if (product.IsDecant)
            {
                // Decant: descuenta del pool de ml, no del stock de variante.
                var mlNeeded = ParseMl(variant.Attributes) * item.Quantity;
                if (mlNeeded <= 0) continue;
                if (product.StockMl < mlNeeded)
                    throw new ConflictException($"Stock insuficiente de '{product.Name}'. Disponible: {product.StockMl} ml, necesita: {mlNeeded} ml.");
                var mlBefore = product.StockMl;
                product.DecrementMl(mlNeeded);
                unitOfWork.Add(StockMovement.Create(
                    product.Id, variant.Id, MovementType.SaleOut,
                    mlNeeded, mlBefore, product.StockMl,
                    referenceId: order.Id, referenceType: "Order",
                    reason: $"Pedido {order.OrderNumber} confirmado ({mlNeeded} ml)",
                    createdByUserId: null));
                continue;
            }

            if (variant.StockQuantity < item.Quantity)
                throw new ConflictException($"Stock insuficiente para '{item.Sku}'. Disponible: {variant.StockQuantity}.");

            var stockBefore = variant.StockQuantity;
            variant.DecrementStock(item.Quantity);

            var movement = StockMovement.Create(
                product.Id, variant.Id, MovementType.SaleOut,
                item.Quantity, stockBefore, variant.StockQuantity,
                referenceId: order.Id,
                referenceType: "Order",
                reason: $"Pedido {order.OrderNumber} confirmado",
                createdByUserId: null);
            unitOfWork.Add(movement);
        }
    }

    private async Task RestoreStockForOrderAsync(Order order, CancellationToken ct)
    {
        foreach (var item in order.Items)
        {
            var product = await productRepository.GetWithVariantsAsync(item.ProductId, ct);
            if (product == null) continue;

            var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId);
            if (variant == null) continue;

            if (product.IsDecant)
            {
                // Decant: devuelve los ml al pool.
                var mlBack = ParseMl(variant.Attributes) * item.Quantity;
                if (mlBack <= 0) continue;
                var mlBefore = product.StockMl;
                product.SetStockMl(product.StockMl + mlBack);
                unitOfWork.Add(StockMovement.Create(
                    product.Id, variant.Id, MovementType.Return,
                    mlBack, mlBefore, product.StockMl,
                    referenceId: order.Id, referenceType: "Order",
                    reason: $"Pedido {order.OrderNumber} cancelado — {mlBack} ml restaurados",
                    createdByUserId: null));
                continue;
            }

            var stockBefore = variant.StockQuantity;
            variant.IncrementStock(item.Quantity);

            var movement = StockMovement.Create(
                product.Id, variant.Id, MovementType.Return,
                item.Quantity, stockBefore, variant.StockQuantity,
                referenceId: order.Id,
                referenceType: "Order",
                reason: $"Pedido {order.OrderNumber} cancelado — stock restaurado",
                createdByUserId: null);
            unitOfWork.Add(movement);
        }
    }

    /// <summary>
    /// Crea una Venta a partir de un pedido pagado, para que impacte en la
    /// facturación y la ganancia. NO descuenta stock: de eso ya se encarga el
    /// pedido al confirmarse (evita el doble descuento).
    /// </summary>
    private async Task CreateSaleFromOrderAsync(Order order, CancellationToken ct)
    {
        var saleNumber = await saleRepository.GenerateSaleNumberAsync(ct);
        var userId = currentUser.UserId ?? Guid.Empty;
        var vialCosts = VialCostSettings.Parse((await appSettings.GetAsync(VialCostSettings.Key, ct))?.Value);

        var sale = Sale.Create(
            saleNumber, userId,
            paymentMethod: order.PaymentMethod ?? "Transfer",
            channel: SaleChannel.WhatsApp,
            currency: order.Currency,
            customerId: order.CustomerId,
            notes: $"Generada desde el pedido {order.OrderNumber}",
            customerName: order.BuyerName);
        unitOfWork.Add(sale);

        foreach (var item in order.Items)
        {
            var product = await productRepository.GetWithVariantsAsync(item.ProductId, ct);
            var variant = product?.Variants.FirstOrDefault(v => v.Id == item.VariantId);

            Money? unitCost = null;
            if (product != null && variant != null)
            {
                if (product.IsDecant)
                {
                    var mlPerUnit = ParseMl(variant.Attributes);
                    var vial = vialCosts.GetValueOrDefault(mlPerUnit, 0m);
                    unitCost = Money.Of(mlPerUnit * (product.CostPerMl ?? 0m) + vial, order.Currency);
                }
                else if (variant.Cost is { } c)
                {
                    unitCost = Money.Of(c.Amount, order.Currency);
                }
            }

            sale.AddItem(item.ProductId, item.VariantId, item.ProductName, item.Sku,
                item.Quantity, item.UnitPrice, item.Discount, unitCost);
        }
    }

    // Extrae los ml de una variante decant desde su atributo "Tamaño" (ej. "5ml" -> 5).
    private static int ParseMl(Dictionary<string, string> attributes)
    {
        if (attributes == null) return 0;
        foreach (var v in attributes.Values)
        {
            var m = System.Text.RegularExpressions.Regex.Match(v ?? "", @"(\d+)\s*ml",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success) return int.Parse(m.Groups[1].Value);
        }
        return 0;
    }
}
