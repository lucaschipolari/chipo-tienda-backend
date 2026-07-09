using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.PurchaseOrders.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;

public record GetPurchaseOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? SupplierId = null,
    string? Status = null
) : IRequest<PagedResult<PurchaseOrderListItemDto>>;

public class GetPurchaseOrdersQueryHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISupplierRepository supplierRepository
) : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderListItemDto>>
{
    public async Task<PagedResult<PurchaseOrderListItemDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken ct)
    {
        var (orders, total) = await purchaseOrderRepository.GetPagedAsync(
            request.Page, request.PageSize,
            supplierId: request.SupplierId,
            status: request.Status,
            ct: ct);

        // Hydrate supplier names — cache to avoid N+1
        var supplierIds = orders.Select(o => o.SupplierId).Distinct().ToList();
        var supplierNames = new Dictionary<Guid, string>();
        foreach (var sid in supplierIds)
        {
            var s = await supplierRepository.GetByIdAsync(sid, ct);
            if (s != null) supplierNames[sid] = s.CompanyName;
        }

        var items = orders.Select(o => new PurchaseOrderListItemDto(
            Id: o.Id,
            PurchaseNumber: o.PurchaseNumber,
            SupplierId: o.SupplierId,
            SupplierName: supplierNames.GetValueOrDefault(o.SupplierId),
            Status: o.Status.ToString(),
            ItemCount: o.Items.Count,
            Total: o.Total.Amount,
            Currency: o.Currency == "PEN" ? "ARS" : o.Currency,
            ExpectedDeliveryDate: o.ExpectedDeliveryDate,
            CreatedAt: o.CreatedAt
        )).ToList();

        return PagedResult<PurchaseOrderListItemDto>.Create(items, total, request.Page, request.PageSize);
    }
}
