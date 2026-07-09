using ChipoBackend.Application.Features.Reports.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Reports.Queries.GetPurchasesReport;

public record GetPurchasesReportQuery(
    DateTime From,
    DateTime To,
    Guid? SupplierId = null
) : IRequest<PurchasesReportDto>;

public class GetPurchasesReportQueryHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISupplierRepository supplierRepository
) : IRequestHandler<GetPurchasesReportQuery, PurchasesReportDto>
{
    public async Task<PurchasesReportDto> Handle(GetPurchasesReportQuery request, CancellationToken ct)
    {
        var (orders, _) = await purchaseOrderRepository.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            supplierId: request.SupplierId,
            ct: ct);

        var filtered = orders
            .Where(o => o.CreatedAt >= request.From && o.CreatedAt <= request.To)
            .ToList();

        // Batch-load supplier names
        var supplierIds = filtered.Select(o => o.SupplierId).Distinct().ToList();
        var supplierNames = new Dictionary<Guid, string?>();
        foreach (var sid in supplierIds)
        {
            var supplier = await supplierRepository.GetByIdAsync(sid, ct);
            supplierNames[sid] = supplier?.CompanyName;
        }

        var rows = filtered.Select(o => new PurchasesReportRowDto(
            PurchaseNumber: o.PurchaseNumber,
            SupplierName: supplierNames.GetValueOrDefault(o.SupplierId) ?? "—",
            Status: o.Status.ToString(),
            Date: o.CreatedAt,
            ItemCount: o.Items.Count,
            Total: o.Total.Amount,
            Currency: o.Total.Currency
        )).ToList();

        var totalCost = filtered
            .Where(o => o.Status is Domain.Entities.Purchasing.PurchaseOrderStatus.Received
                     or Domain.Entities.Purchasing.PurchaseOrderStatus.PartiallyReceived)
            .Sum(o => o.Total.Amount);
        var currency = filtered.FirstOrDefault()?.Total.Currency ?? "ARS";

        return new PurchasesReportDto(
            TotalCount: filtered.Count,
            TotalSpent: totalCost,
            Currency: currency,
            Rows: rows
        );
    }
}
