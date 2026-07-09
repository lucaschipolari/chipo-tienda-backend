using ChipoBackend.Application.Features.Finance.DTOs;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Finance.Queries.GetCashFlow;

// ── New query (Granularity-based, returns CashFlowDto) ────────────────────────

/// <summary>
/// Returns a cash-flow report grouped by the requested granularity.
/// Granularity options: "daily" | "weekly" | "monthly"
/// </summary>
public record GetCashFlowQuery(
    string Granularity = "daily",
    DateTime? From = null,
    DateTime? To = null
) : IRequest<CashFlowDto>;

public class GetCashFlowQueryHandler(
    ISaleRepository saleRepository,
    IExpenseRepository expenseRepository,
    IPurchaseOrderRepository purchaseOrderRepository
) : IRequestHandler<GetCashFlowQuery, CashFlowDto>
{
    private const string Currency = "ARS";

    public async Task<CashFlowDto> Handle(GetCashFlowQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        var from = DateTime.SpecifyKind((request.From ?? today.AddDays(-29)).Date, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind((request.To ?? today).Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        // Fetch all data in range
        var salesSummary = await saleRepository.GetSummaryAsync(from, to, ct);

        var expenses = await expenseRepository.GetByDateRangeAsync(from, to, ct);
        var validExpenses = expenses.Where(e => e.Status != ExpenseStatus.Cancelled).ToList();

        var (allPurchases, _) = await purchaseOrderRepository.GetPagedAsync(
            1, 5000, status: nameof(PurchaseOrderStatus.Received), ct: ct);
        var purchases = allPurchases
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .ToList();

        // Build lookup dictionaries by date
        var salesByDay = salesSummary.ByDay
            .ToDictionary(d => d.Date.Date, d => d.Revenue);

        var expensesByDay = validExpenses
            .GroupBy(e => e.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount.Amount));

        var purchasesByDay = purchases
            .GroupBy(p => p.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Total.Amount));

        // Build period buckets
        var periods = BuildPeriods(from, to, request.Granularity);

        decimal runningBalance = 0m;
        var entries = periods.Select(period =>
        {
            var inflows = SumByDayRange(salesByDay, period.Start, period.End);
            var costOutflows = SumByDayRange(purchasesByDay, period.Start, period.End);
            var expenseOutflows = SumByDayRange(expensesByDay, period.Start, period.End);
            var outflows = costOutflows + expenseOutflows;
            var balance = inflows - outflows;
            runningBalance += balance;

            return new CashFlowEntryDto(
                Date: period.Label,
                Inflows: inflows,
                Outflows: outflows,
                Balance: balance,
                RunningBalance: runningBalance);
        }).ToList();

        return new CashFlowDto(
            Entries: entries,
            TotalInflows: entries.Sum(e => e.Inflows),
            TotalOutflows: entries.Sum(e => e.Outflows),
            NetBalance: entries.Sum(e => e.Balance),
            Currency: Currency);
    }

    private static decimal SumByDayRange(Dictionary<DateTime, decimal> byDay, DateTime start, DateTime end)
    {
        var total = 0m;
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            if (byDay.TryGetValue(d, out var v))
                total += v;
        }
        return total;
    }

    private static List<(string Label, DateTime Start, DateTime End)> BuildPeriods(
        DateTime from, DateTime to, string granularity)
    {
        var result = new List<(string, DateTime, DateTime)>();

        switch (granularity.ToLowerInvariant())
        {
            case "monthly":
            {
                var current = new DateTime(from.Year, from.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                while (current <= to)
                {
                    var periodEnd = current.AddMonths(1).AddTicks(-1);
                    if (periodEnd > to) periodEnd = to;
                    result.Add((current.ToString("yyyy-MM"), current, periodEnd));
                    current = current.AddMonths(1);
                }
                break;
            }
            case "weekly":
            {
                // Align to Monday
                var dow = (int)from.Date.DayOfWeek;
                var offset = dow == 0 ? -6 : 1 - dow;
                var current = from.Date.AddDays(offset).ToUniversalTime();
                while (current <= to)
                {
                    var periodEnd = current.AddDays(7).AddTicks(-1);
                    if (periodEnd > to) periodEnd = to;
                    result.Add((current.ToString("yyyy-MM-dd"), current, periodEnd));
                    current = current.AddDays(7);
                }
                break;
            }
            default: // "daily"
            {
                var current = from.Date.ToUniversalTime();
                while (current <= to)
                {
                    var periodEnd = current.AddDays(1).AddTicks(-1);
                    result.Add((current.ToString("yyyy-MM-dd"), current, periodEnd));
                    current = current.AddDays(1);
                }
                break;
            }
        }

        return result;
    }
}

// ── Legacy query (kept for backward compatibility) ────────────────────────────

/// <summary>
/// Legacy cash-flow query. Prefer <see cref="GetCashFlowQuery"/> for new code.
/// </summary>
public record GetCashFlowReportQuery(
    DateTime From,
    DateTime To,
    string GroupBy = "day"   // "day" | "week" | "month"
) : IRequest<CashFlowReportDto>;

public class GetCashFlowReportQueryHandler(
    ISaleRepository saleRepository,
    IExpenseRepository expenseRepository,
    IPurchaseOrderRepository purchaseOrderRepository
) : IRequestHandler<GetCashFlowReportQuery, CashFlowReportDto>
{
    public async Task<CashFlowReportDto> Handle(GetCashFlowReportQuery request, CancellationToken ct)
    {
        var from = DateTime.SpecifyKind(request.From.Date, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(request.To.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var (sales, _) = await saleRepository.GetPagedAsync(1, 5000, from: from, to: to, ct: ct);
        var expenses = await expenseRepository.GetByDateRangeAsync(from, to, ct);
        var (allPurchases, _) = await purchaseOrderRepository.GetPagedAsync(
            1, 5000, status: nameof(PurchaseOrderStatus.Received), ct: ct);
        var purchases = allPurchases
            .Where(p => p.UpdatedAt >= from && p.UpdatedAt <= to)
            .ToList();

        var periods = BuildPeriods(from, to, request.GroupBy);

        decimal runningBalance = 0m;
        var entries = new List<CashFlowEntryDto>();

        foreach (var (periodLabel, periodStart, periodEnd) in periods)
        {
            var inflows = sales
                .Where(s => s.CreatedAt >= periodStart && s.CreatedAt <= periodEnd)
                .Sum(s => s.Total.Amount);

            var expenseOutflows = expenses
                .Where(e => e.Date >= periodStart && e.Date <= periodEnd)
                .Sum(e => e.Amount.Amount);

            var purchaseOutflows = purchases
                .Where(p => p.UpdatedAt >= periodStart && p.UpdatedAt <= periodEnd)
                .Sum(p => p.Total.Amount);

            var outflows = expenseOutflows + purchaseOutflows;
            var balance = inflows - outflows;
            runningBalance += balance;

            entries.Add(new CashFlowEntryDto(
                Date: periodLabel,
                Inflows: inflows,
                Outflows: outflows,
                Balance: balance,
                RunningBalance: runningBalance));
        }

        return new CashFlowReportDto(
            TotalInflows: entries.Sum(e => e.Inflows),
            TotalOutflows: entries.Sum(e => e.Outflows),
            NetCashFlow: entries.Sum(e => e.Balance),
            Entries: entries);
    }

    private static List<(string Label, DateTime Start, DateTime End)> BuildPeriods(
        DateTime from, DateTime to, string groupBy)
    {
        var result = new List<(string, DateTime, DateTime)>();

        switch (groupBy.ToLowerInvariant())
        {
            case "month":
            {
                var current = new DateTime(from.Year, from.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                while (current <= to)
                {
                    var periodEnd = current.AddMonths(1).AddTicks(-1);
                    result.Add((current.ToString("yyyy-MM"), current, periodEnd < to ? periodEnd : to));
                    current = current.AddMonths(1);
                }
                break;
            }
            case "week":
            {
                var current = from.Date.ToUniversalTime();
                var dow = (int)current.DayOfWeek;
                var offset = dow == 0 ? -6 : 1 - dow;
                current = current.AddDays(offset);
                while (current <= to)
                {
                    var periodEnd = current.AddDays(7).AddTicks(-1);
                    result.Add((current.ToString("yyyy-MM-dd"), current, periodEnd < to ? periodEnd : to));
                    current = current.AddDays(7);
                }
                break;
            }
            default: // "day"
            {
                var current = from.Date.ToUniversalTime();
                while (current <= to)
                {
                    var periodEnd = current.AddDays(1).AddTicks(-1);
                    result.Add((current.ToString("yyyy-MM-dd"), current, periodEnd));
                    current = current.AddDays(1);
                }
                break;
            }
        }

        return result;
    }
}
