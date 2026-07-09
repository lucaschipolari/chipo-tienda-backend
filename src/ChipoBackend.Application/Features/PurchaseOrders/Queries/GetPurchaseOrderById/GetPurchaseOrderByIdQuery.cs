using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.PurchaseOrders.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<PurchaseOrderDto>;

public class GetPurchaseOrderByIdQueryHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISupplierRepository supplierRepository,
    IProductRepository productRepository
) : IRequestHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto>
{
    public async Task<PurchaseOrderDto> Handle(GetPurchaseOrderByIdQuery request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        var supplier = await supplierRepository.GetByIdAsync(order.SupplierId, ct);

        // Hidratar nombres de producto y SKU de variante
        var productNames = new Dictionary<Guid, string>();
        var variantSkus = new Dictionary<Guid, string>();
        foreach (var pid in order.Items.Select(i => i.ProductId).Distinct())
        {
            var product = await productRepository.GetWithVariantsAsync(pid, ct);
            if (product is null) continue;
            productNames[pid] = product.Name;
            foreach (var v in product.Variants)
                variantSkus[v.Id] = v.Sku;
        }

        return new PurchaseOrderDto(
            Id: order.Id,
            PurchaseNumber: order.PurchaseNumber,
            SupplierId: order.SupplierId,
            SupplierName: supplier?.CompanyName,
            Status: order.Status.ToString(),
            ExpectedDeliveryDate: order.ExpectedDeliveryDate,
            Subtotal: order.Subtotal.Amount,
            TaxAmount: order.TaxAmount.Amount,
            Total: order.Total.Amount,
            Currency: order.Currency == "PEN" ? "ARS" : order.Currency,
            Notes: order.Notes,
            Items: order.Items.Select(i => new PurchaseOrderItemDto(
                Id: i.Id,
                ProductId: i.ProductId,
                ProductName: productNames.GetValueOrDefault(i.ProductId),
                VariantId: i.VariantId,
                VariantSku: variantSkus.GetValueOrDefault(i.VariantId),
                Quantity: i.QuantityOrdered,
                QuantityReceived: i.QuantityReceived,
                PendingQuantity: i.PendingQuantity,
                IsFullyReceived: i.IsFullyReceived,
                UnitCost: i.UnitCost.Amount,
                Total: i.Total.Amount,
                Currency: i.UnitCost.Currency == "PEN" ? "ARS" : i.UnitCost.Currency
            )).ToList(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt
        );
    }
}
