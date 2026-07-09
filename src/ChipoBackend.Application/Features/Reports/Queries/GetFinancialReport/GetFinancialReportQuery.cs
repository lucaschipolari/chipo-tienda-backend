using ChipoBackend.Application.Features.Reports.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Reports.Queries.GetFinancialReport;

public record GetFinancialReportQuery(
    DateTime From,
    DateTime To,
    string? GroupBy = "day"  // "day" | "month"
) : IRequest<FinancialReportDto>;

public class GetFinancialReportQueryHandler(
    ISaleRepository saleRepository,
    IExpenseRepository expenseRepository,
    IPurchaseOrderRepository purchaseOrderRepository
) : IRequestHandler<GetFinancialReportQuery, FinancialReportDto>
{
    public async Task<FinancialReportDto> Handle(GetFinancialReportQuery request, CancellationToken ct)
    {
        // Revenue from sales
        var (sales, _) = await saleRepository.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            from: request.From,
            to: request.To,
            ct: ct);

        // Expenses
        var (expenses, _) = await expenseRepository.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            from: request.From,
            to: request.To,
            ct: ct);

        // Purchase costs
        var (purchases, _) = await purchaseOrderRepository.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            ct: ct);

        // Solo compras efectivamente recibidas cuentan como costo real
        var filteredPurchases = purchases
            .Where(p => p.CreatedAt >= request.From && p.CreatedAt <= request.To)
            .Where(p => p.Status is Domain.Entities.Purchasing.PurchaseOrderStatus.Received
                     or Domain.Entities.Purchasing.PurchaseOrderStatus.PartiallyReceived)
            .ToList();

        var totalRevenue = sales.Sum(s => s.Total.Amount);
        var totalExpenses = expenses.Sum(e => e.Amount.Amount);
        var totalPurchaseCosts = filteredPurchases.Sum(p => p.Total.Amount);
        var netProfit = totalRevenue - totalExpenses - totalPurchaseCosts;
        var currency = sales.FirstOrDefault()?.Total.Currency ?? "ARS";

        // Build time-series lines grouped by day or month
        var lines = BuildLines(
            request.From,
            request.To,
            request.GroupBy ?? "day",
            sales.ToList(),
            expenses.ToList(),
            filteredPurchases);

        return new FinancialReportDto(
            TotalRevenue: totalRevenue,
            TotalExpenses: totalExpenses,
            TotalPurchaseCosts: totalPurchaseCosts,
            NetProfit: netProfit,
            Currency: currency,
            Lines: lines
        );
    }

    private static List<FinancialReportLineDto> BuildLines(
        DateTime from,
        DateTime to,
        string groupBy,
        List<ChipoBackend.Domain.Entities.Sales.Sale> sales,
        List<ChipoBackend.Domain.Entities.Expenses.Expense> expenses,
        List<ChipoBackend.Domain.Entities.Purchasing.PurchaseOrder> purchases)
    {
        var lines = new List<FinancialReportLineDto>();

        if (groupBy == "month")
        {
            var current = new DateTime(from.Year, from.Month, 1);
            while (current <= to)
            {
                var next = current.AddMonths(1);
                var label = current.ToString("yyyy-MM");

                var inflows = sales
                    .Where(s => s.CreatedAt >= current && s.CreatedAt < next)
                    .Sum(s => s.Total.Amount);

                var outflows = expenses
                    .Where(e => e.Date >= current && e.Date < next)
                    .Sum(e => e.Amount.Amount)
                    + purchases
                    .Where(p => p.CreatedAt >= current && p.CreatedAt < next)
                    .Sum(p => p.Total.Amount);

                lines.Add(new FinancialReportLineDto(label, inflows, outflows, inflows - outflows));
                current = next;
            }
        }
        else // day
        {
            var current = from.Date;
            while (current <= to.Date)
            {
                var next = current.AddDays(1);
                var label = current.ToString("yyyy-MM-dd");

                var inflows = sales
                    .Where(s => s.CreatedAt >= current && s.CreatedAt < next)
                    .Sum(s => s.Total.Amount);

                var outflows = expenses
                    .Where(e => e.Date >= current && e.Date < next)
                    .Sum(e => e.Amount.Amount)
                    + purchases
                    .Where(p => p.CreatedAt >= current && p.CreatedAt < next)
                    .Sum(p => p.Total.Amount);

                lines.Add(new FinancialReportLineDto(label, inflows, outflows, inflows - outflows));
                current = next;
            }
        }

        return lines;
    }
}
