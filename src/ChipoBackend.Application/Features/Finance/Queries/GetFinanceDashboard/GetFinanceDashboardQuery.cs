using ChipoBackend.Application.Features.Finance.DTOs;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Finance.Queries.GetFinanceDashboard;

/// <summary>
/// Returns a finance dashboard for the requested period.
/// Period options: "today" | "week" | "month" | "year" | "custom"
/// When Period = "custom", From and To must be provided.
/// </summary>
public record GetFinanceDashboardQuery(
    DateTime? From = null,
    DateTime? To = null,
    string Period = "month"
) : IRequest<FinanceDashboardDto>;

public class GetFinanceDashboardQueryHandler(
    ISaleRepository saleRepository,
    IExpenseRepository expenseRepository,
    IPurchaseOrderRepository purchaseOrderRepository
) : IRequestHandler<GetFinanceDashboardQuery, FinanceDashboardDto>
{
    private const string Currency = "ARS";

    public async Task<FinanceDashboardDto> Handle(GetFinanceDashboardQuery request, CancellationToken ct)
    {
        var (from, to) = ResolveDateRange(request);

        // ── Sales data ────────────────────────────────────────────────────────
        var salesSummary = await saleRepository.GetSummaryAsync(from, to, ct);

        // Previous period for comparison
        var periodLength = to - from;
        var prevFrom = from - periodLength - TimeSpan.FromTicks(1);
        var prevTo = from - TimeSpan.FromTicks(1);
        var prevSalesSummary = await saleRepository.GetSummaryAsync(prevFrom, prevTo, ct);

        var totalRevenue = salesSummary.TotalRevenue;
        var prevRevenue = prevSalesSummary.TotalRevenue;
        var revenueVsPrev = prevRevenue > 0
            ? Math.Round((totalRevenue - prevRevenue) / prevRevenue * 100, 2)
            : 0m;

        // ── Purchase costs ────────────────────────────────────────────────────
        var (allPurchases, _) = await purchaseOrderRepository.GetPagedAsync(
            1, 1000, status: nameof(PurchaseOrderStatus.Received), ct: ct);

        var totalCosts = allPurchases
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .Sum(p => p.Total.Amount);

        // ── Expenses ──────────────────────────────────────────────────────────
        var expenses = await expenseRepository.GetByDateRangeAsync(from, to, ct);
        var totalExpenses = expenses
            .Where(e => e.Status != ExpenseStatus.Cancelled)
            .Sum(e => e.Amount.Amount);

        // ── KPI calculations ──────────────────────────────────────────────────
        var grossProfit = totalRevenue - totalCosts;
        var netProfit = grossProfit - totalExpenses;
        var grossMargin = totalRevenue > 0
            ? Math.Round(grossProfit / totalRevenue * 100, 2)
            : 0m;
        var netMargin = totalRevenue > 0
            ? Math.Round(netProfit / totalRevenue * 100, 2)
            : 0m;

        var kpis = new FinancialKpiDto(
            TotalRevenue: totalRevenue,
            TotalCosts: totalCosts,
            TotalExpenses: totalExpenses,
            GrossProfit: grossProfit,
            NetProfit: netProfit,
            GrossMargin: grossMargin,
            NetMargin: netMargin,
            AverageTicket: salesSummary.AverageTicket,
            TotalSales: salesSummary.TotalSales,
            RevenueVsPreviousPeriod: revenueVsPrev,
            Currency: Currency);

        // ── Revenue by day ────────────────────────────────────────────────────
        var expensesByDay = expenses
            .Where(e => e.Status != ExpenseStatus.Cancelled)
            .GroupBy(e => e.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount.Amount));

        var purchasesByDay = allPurchases
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .GroupBy(p => p.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Total.Amount));

        var revenueByDay = salesSummary.ByDay
            .Select(d =>
            {
                var day = d.Date.Date;
                expensesByDay.TryGetValue(day, out var dayExpenses);
                purchasesByDay.TryGetValue(day, out var dayCosts);
                return new RevenueByDayDto(
                    Date: day.ToString("yyyy-MM-dd"),
                    Revenue: d.Revenue,
                    Costs: dayCosts,
                    Expenses: dayExpenses);
            })
            .ToList();

        // ── Cash flow (daily) ─────────────────────────────────────────────────
        var allDays = Enumerable
            .Range(0, (int)(to.Date - from.Date).TotalDays + 1)
            .Select(i => from.Date.AddDays(i))
            .ToList();

        var salesByDay = salesSummary.ByDay
            .ToDictionary(d => d.Date.Date, d => d.Revenue);

        decimal runningBalance = 0m;
        var cashFlowEntries = allDays.Select(day =>
        {
            salesByDay.TryGetValue(day, out var dayRevenue);
            expensesByDay.TryGetValue(day, out var dayExpenses);
            purchasesByDay.TryGetValue(day, out var dayCosts);
            var outflows = dayCosts + dayExpenses;
            var balance = dayRevenue - outflows;
            runningBalance += balance;
            return new CashFlowEntryDto(
                Date: day.ToString("yyyy-MM-dd"),
                Inflows: dayRevenue,
                Outflows: outflows,
                Balance: balance,
                RunningBalance: runningBalance);
        }).ToList();

        var cashFlow = new CashFlowDto(
            Entries: cashFlowEntries,
            TotalInflows: cashFlowEntries.Sum(e => e.Inflows),
            TotalOutflows: cashFlowEntries.Sum(e => e.Outflows),
            NetBalance: cashFlowEntries.Sum(e => e.Balance),
            Currency: Currency);

        // ── Top products ──────────────────────────────────────────────────────
        var topProducts = salesSummary.TopProducts
            .Select(p => new FinanceTopProductDto(
                ProductName: p.ProductName,
                Revenue: p.Revenue,
                Cost: 0m,
                Profit: p.Revenue,
                Margin: 100m,
                Quantity: p.Quantity))
            .ToList();

        return new FinanceDashboardDto(
            Kpis: kpis,
            CashFlow: cashFlow,
            RevenueByDay: revenueByDay,
            TopProducts: topProducts,
            Period: request.Period,
            From: from,
            To: to);
    }

    private static (DateTime From, DateTime To) ResolveDateRange(GetFinanceDashboardQuery request)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        return request.Period.ToLowerInvariant() switch
        {
            "today" => (
                DateTime.SpecifyKind(today, DateTimeKind.Utc),
                DateTime.SpecifyKind(today.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),

            "week" => (
                DateTime.SpecifyKind(StartOfWeek(today), DateTimeKind.Utc),
                DateTime.SpecifyKind(today.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),

            "month" => (
                DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc),
                DateTime.SpecifyKind(today.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),

            "year" => (
                DateTime.SpecifyKind(new DateTime(today.Year, 1, 1), DateTimeKind.Utc),
                DateTime.SpecifyKind(today.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),

            "custom" => (
                DateTime.SpecifyKind(
                    (request.From ?? today).Date, DateTimeKind.Utc),
                DateTime.SpecifyKind(
                    (request.To ?? today).Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)),

            // default: month
            _ => (
                DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc),
                DateTime.SpecifyKind(today.AddDays(1).AddTicks(-1), DateTimeKind.Utc))
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var dow = (int)date.DayOfWeek;
        var offset = dow == 0 ? -6 : 1 - dow; // Monday-based week
        return date.AddDays(offset);
    }
}
