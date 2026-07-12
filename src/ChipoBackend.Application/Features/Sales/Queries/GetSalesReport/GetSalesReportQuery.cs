using ChipoBackend.Application.Features.Sales.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Queries.GetSalesReport;

public record GetSalesReportQuery(
    DateTime From,
    DateTime To
) : IRequest<SalesReportDto>;

public class GetSalesReportQueryHandler(
    ISaleRepository saleRepository,
    ICustomerRepository customerRepository
) : IRequestHandler<GetSalesReportQuery, SalesReportDto>
{
    public async Task<SalesReportDto> Handle(GetSalesReportQuery request, CancellationToken ct)
    {
        var summary = await saleRepository.GetSummaryAsync(request.From, request.To, ct);

        // Calcular variación respecto al período anterior equivalente
        var periodDays = (request.To - request.From).TotalDays;
        var prevFrom = request.From.AddDays(-periodDays);
        var prevTo = request.From.AddDays(-1);
        var prevSummary = await saleRepository.GetSummaryAsync(prevFrom, prevTo, ct);

        double variationPct = 0;
        if (prevSummary.TotalRevenue > 0)
            variationPct = (double)((summary.TotalRevenue - prevSummary.TotalRevenue) / prevSummary.TotalRevenue * 100);

        // Enriquecer top customers con nombres
        var topCustomers = new List<TopCustomerDto>();
        foreach (var tc in summary.TopCustomers)
        {
            var customer = await customerRepository.GetByIdAsync(tc.CustomerId, ct);
            topCustomers.Add(new TopCustomerDto(
                CustomerId: tc.CustomerId,
                CustomerName: customer?.FullName ?? "—",
                TotalOrders: tc.Orders,
                TotalSpent: tc.Total
            ));
        }

        return new SalesReportDto(
            From: request.From,
            To: request.To,
            TotalSales: summary.TotalSales,
            TotalRevenue: summary.TotalRevenue,
            TotalCost: summary.TotalCost,
            TotalProfit: summary.TotalProfit,
            AverageTicket: summary.AverageTicket,
            RevenueVsPreviousPeriod: (decimal)variationPct,
            ByDay: summary.ByDay.Select(d => new DailyRevenueDto(d.Date, d.Revenue, d.Count)).ToList(),
            TopProducts: summary.TopProducts.Select(p => new TopProductDto(p.ProductId, p.ProductName, p.Quantity, p.Revenue)).ToList(),
            TopCustomers: topCustomers
        );
    }
}
