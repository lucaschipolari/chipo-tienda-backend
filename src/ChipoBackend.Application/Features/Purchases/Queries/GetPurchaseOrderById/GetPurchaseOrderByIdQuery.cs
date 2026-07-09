using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Purchases.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Purchases.Queries.GetPurchaseOrderById;

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

        // Batch-load unique products for item enrichment
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
        var productMap = new Dictionary<Guid, (string? Name, Dictionary<Guid, string> VariantSkus)>();
        foreach (var pid in productIds)
        {
            var product = await productRepository.GetWithVariantsAsync(pid, ct);
            if (product != null)
            {
                var skuMap = product.Variants.ToDictionary(v => v.Id, v => v.Sku);
                productMap[pid] = (product.Name, skuMap);
            }
        }

        var itemDtos = order.Items.Select(i =>
        {
            productMap.TryGetValue(i.ProductId, out var productInfo);
            var variantSku = productInfo.VariantSkus?.GetValueOrDefault(i.VariantId);

            return new PurchaseOrderItemDto(
                Id: i.Id,
                ProductId: i.ProductId,
                ProductName: productInfo.Name,
                VariantId: i.VariantId,
                VariantSku: variantSku,
                Quantity: i.QuantityOrdered,
                QuantityReceived: i.QuantityReceived,
                UnitCost: i.UnitCost.Amount,
                Total: i.Total.Amount,
                Currency: i.UnitCost.Currency,
                IsFullyReceived: i.IsFullyReceived
            );
        }).ToList();

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
            Currency: order.Total.Currency,
            Notes: order.Notes,
            Items: itemDtos,
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt
        );
    }
}
