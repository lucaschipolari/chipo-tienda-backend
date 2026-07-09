using ChipoBackend.Application.Features.Reports.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Reports.Queries.GetSalesReport;

public record GetSalesReportQuery(
    DateTime From,
    DateTime To,
    Guid? CustomerId = null,
    string? Channel = null,
    string? Search = null
) : IRequest<SalesReportDto>;

public class GetSalesReportQueryHandler(
    ISaleRepository saleRepository,
    ICustomerRepository customerRepository
) : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken ct)
    {
        var (sales, _) = await saleRepository.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            customerId: request.CustomerId,
            from: request.From,
            to: request.To,
            ct: ct);

        var filteredSales = sales.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Channel))
            filteredSales = filteredSales.Where(s => s.Channel.ToString() == request.Channel);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLowerInvariant();
            filteredSales = filteredSales.Where(s =>
                s.SaleNumber.ToLowerInvariant().Contains(search));
        }

        var salesList = filteredSales.ToList();

        // Batch-load customer names
        var customerIds = salesList
            .Where(s => s.CustomerId.HasValue)
            .Select(s => s.CustomerId!.Value)
            .Distinct()
            .ToList();

        var customerNames = new Dictionary<Guid, string>();
        foreach (var cid in customerIds)
        {
            var customer = await customerRepository.GetByIdAsync(cid, ct);
            if (customer is not null)
                customerNames[cid] = customer.FullName;
        }

        var rows = salesList.Select(s => new SalesReportRowDto(
            Date: s.CreatedAt,
            SaleNumber: s.SaleNumber,
            BuyerName: s.CustomerId.HasValue
                ? customerNames.GetValueOrDefault(s.CustomerId.Value, "—")
                : "—",
            Channel: s.Channel.ToString(),
            PaymentMethod: s.PaymentMethod,
            ItemCount: s.Items.Count,
            Subtotal: s.Subtotal.Amount,
            Discount: s.DiscountAmount.Amount,
            Tax: 0m,
            Total: s.Total.Amount,
            Currency: s.Total.Currency
        )).ToList();

        var totalRevenue = salesList.Sum(s => s.Total.Amount);
        var totalDiscount = salesList.Sum(s => s.DiscountAmount.Amount);
        var avgTicket = salesList.Count > 0 ? totalRevenue / salesList.Count : 0m;
        var currency = salesList.FirstOrDefault()?.Total.Currency ?? "ARS";
        var period = $"{request.From:yyyy-MM-dd} / {request.To:yyyy-MM-dd}";

        return new SalesReportDto(
            Period: period,
            TotalCount: salesList.Count,
            TotalRevenue: totalRevenue,
            TotalDiscount: totalDiscount,
            TotalTax: 0m,
            AverageTicket: avgTicket,
            Currency: currency,
            Rows: rows
        );
    }
}
