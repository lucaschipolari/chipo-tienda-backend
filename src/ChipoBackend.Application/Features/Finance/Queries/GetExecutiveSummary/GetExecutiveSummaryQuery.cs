using ChipoBackend.Application.Features.Finance.DTOs;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Finance.Queries.GetExecutiveSummary;

public record GetExecutiveSummaryQuery(
    DateTime From,
    DateTime To
) : IRequest<ExecutiveSummaryDto>;

public class GetExecutiveSummaryQueryHandler(
    ISaleRepository saleRepository,
    IExpenseRepository expenseRepository,
    IPurchaseOrderRepository purchaseOrderRepository
) : IRequestHandler<GetExecutiveSummaryQuery, ExecutiveSummaryDto>
{
    public async Task<ExecutiveSummaryDto> Handle(GetExecutiveSummaryQuery request, CancellationToken ct)
    {
        var from = DateTime.SpecifyKind(request.From.Date, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(request.To.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Fetch all data in range
        var (sales, _) = await saleRepository.GetPagedAsync(1, 5000, from: from, to: to, ct: ct);
        var expenses = await expenseRepository.GetByDateRangeAsync(from, to, ct);
        var (allPurchases, _) = await purchaseOrderRepository.GetPagedAsync(1, 5000, status: nameof(PurchaseOrderStatus.Received), ct: ct);
        var purchases = allPurchases
            .Where(p => p.UpdatedAt >= from && p.UpdatedAt <= to)
            .ToList();

        var totalRevenue = sales.Sum(s => s.Total.Amount);
        var totalExpenses = expenses.Sum(e => e.Amount.Amount);
        var totalPurchaseCosts = purchases.Sum(p => p.Total.Amount);
        var grossProfit = totalRevenue - totalPurchaseCosts;
        var netProfit = grossProfit - totalExpenses;
        var overallMargin = totalRevenue > 0 ? Math.Round(netProfit / totalRevenue * 100, 2) : 0m;

        // Average monthly figures
        var totalMonths = Math.Max(1, (int)Math.Ceiling((to - from).TotalDays / 30.0));
        var avgMonthlyRevenue = Math.Round(totalRevenue / totalMonths, 2);
        var avgMonthlyExpenses = Math.Round(totalExpenses / totalMonths, 2);

        // By period (monthly)
        var byPeriod = new List<FinancialPeriodDto>();
        var periodCursor = new DateTime(from.Year, from.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        while (periodCursor <= to)
        {
            var periodEnd = periodCursor.AddMonths(1).AddTicks(-1);
            var effectiveEnd = periodEnd < to ? periodEnd : to;
            var label = periodCursor.ToString("yyyy-MM");

            var periodRevenue = sales
                .Where(s => s.CreatedAt >= periodCursor && s.CreatedAt <= effectiveEnd)
                .Sum(s => s.Total.Amount);

            var periodExpenses = expenses
                .Where(e => e.Date >= periodCursor && e.Date <= effectiveEnd)
                .Sum(e => e.Amount.Amount);

            var periodPurchases = purchases
                .Where(p => p.UpdatedAt >= periodCursor && p.UpdatedAt <= effectiveEnd)
                .Sum(p => p.Total.Amount);

            var periodProfit = periodRevenue - periodExpenses - periodPurchases;
            var periodMargin = periodRevenue > 0 ? Math.Round(periodProfit / periodRevenue * 100, 2) : 0m;

            byPeriod.Add(new FinancialPeriodDto(label, periodRevenue, periodExpenses, periodPurchases, periodProfit, periodMargin));
            periodCursor = periodCursor.AddMonths(1);
        }

        // Top & bottom products by revenue (in-memory grouping from sale items)
        var productGroups = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto(
                ProductId: g.Key.ProductId,
                ProductName: g.Key.ProductName,
                UnitsSold: g.Sum(i => i.Quantity),
                Revenue: g.Sum(i => i.Total.Amount),
                EstimatedCost: 0m,
                GrossProfit: g.Sum(i => i.Total.Amount),
                Margin: 100m
            ))
            .ToList();

        var topProducts = productGroups
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        var bottomProducts = productGroups
            .OrderBy(p => p.Revenue)
            .Take(10)
            .ToList();

        return new ExecutiveSummaryDto(
            TotalRevenue: totalRevenue,
            TotalExpenses: totalExpenses,
            TotalPurchaseCosts: totalPurchaseCosts,
            GrossProfit: grossProfit,
            NetProfit: netProfit,
            OverallMargin: overallMargin,
            AverageMonthlyRevenue: avgMonthlyRevenue,
            AverageMonthlyExpenses: avgMonthlyExpenses,
            ByPeriod: byPeriod,
            TopProducts: topProducts,
            BottomProducts: bottomProducts
        );
    }
}
