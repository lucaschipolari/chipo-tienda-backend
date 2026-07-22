using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.Settings;
using ChipoBackend.Domain.Entities.Orders;
using ChipoBackend.Domain.Entities.Sales;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using MediatR;

namespace ChipoBackend.Application.Features.Orders.Commands.GenerateSaleFromOrder;

/// <summary>
/// Genera manualmente la venta de un pedido (para pedidos que no pasaron por el
/// estado "Pagado" y por eso no la generaron solos). Idempotente: si el pedido
/// ya tiene una venta asociada, no crea otra. No descuenta stock (lo maneja el
/// pedido al confirmarse).
/// </summary>
public record GenerateSaleFromOrderCommand(Guid OrderId) : IRequest<Guid>;

public class GenerateSaleFromOrderCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ISaleRepository saleRepository,
    IAppSettingRepository appSettings,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<GenerateSaleFromOrderCommand, Guid>
{
    public async Task<Guid> Handle(GenerateSaleFromOrderCommand request, CancellationToken ct)
    {
        var order = await orderRepository.GetWithItemsAsync(request.OrderId, ct)
            ?? throw new NotFoundException($"Pedido '{request.OrderId}' no encontrado.");

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Refunded)
            throw new ConflictException("No se puede generar una venta de un pedido cancelado o reembolsado.");
        if (order.Status < OrderStatus.Confirmed)
            throw new ConflictException("El pedido debe estar al menos confirmado para generar su venta.");

        var existing = await saleRepository.GetByOrderIdAsync(order.Id, ct);
        if (existing != null)
            return existing.Id; // idempotente

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
        sale.LinkOrder(order.Id);
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

        await unitOfWork.SaveChangesAsync(ct);
        return sale.Id;
    }

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
