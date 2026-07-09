using ChipoBackend.Application.Features.Expenses.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Queries.GetExpenseDashboard;

public record GetExpenseDashboardQuery(DateTime? From = null, DateTime? To = null) : IRequest<ExpenseDashboardDto>;

public class GetExpenseDashboardQueryHandler(IExpenseRepository expenseRepository)
    : IRequestHandler<GetExpenseDashboardQuery, ExpenseDashboardDto>
{
    public async Task<ExpenseDashboardDto> Handle(GetExpenseDashboardQuery request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var rangeFrom = request.From?.Date ?? new DateTime(today.Year, 1, 1);
        var rangeTo = (request.To?.Date ?? today).AddDays(1);

        var (expenses, categories) = await expenseRepository.GetDashboardDataAsync(rangeFrom, rangeTo, ct);

        // Today
        var todayTotal = expenses
            .Where(e => e.Date == today)
            .Sum(e => e.Amount.Amount);

        // Current week (Monday-based)
        var dayOfWeek = (int)today.DayOfWeek;
        var weekStart = today.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
        var weekTotal = expenses
            .Where(e => e.Date >= weekStart && e.Date <= today)
            .Sum(e => e.Amount.Amount);

        // Current month
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthTotal = expenses
            .Where(e => e.Date >= monthStart && e.Date <= today)
            .Sum(e => e.Amount.Amount);

        // Full range total
        var yearTotal = expenses.Sum(e => e.Amount.Amount);

        // By category
        var categoryDict = categories.ToDictionary(c => c.Id);
        var byCategory = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g =>
            {
                var total = g.Sum(e => e.Amount.Amount);
                categoryDict.TryGetValue(g.Key, out var cat);
                return new
                {
                    CategoryId = g.Key,
                    CategoryName = cat?.Name ?? string.Empty,
                    Color = cat?.Color ?? "#6B7280",
                    Total = total,
                    Count = g.Count()
                };
            })
            .ToList();

        var grandTotal = byCategory.Sum(x => x.Total);
        var byCategoryDtos = byCategory
            .Select(x => new ExpenseByCategoryDto(
                x.CategoryId,
                x.CategoryName,
                x.Color,
                x.Total,
                x.Count,
                grandTotal == 0 ? 0 : Math.Round(x.Total / grandTotal * 100, 2)))
            .OrderByDescending(x => x.Total)
            .ToList();

        // Monthly trend: last 6 months
        var sixMonthsAgo = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        var expensesByMonth = expenses
            .Where(e => e.Date >= sixMonthsAgo)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(e => e.Amount.Amount));

        var monthlyTrend = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var month = sixMonthsAgo.AddMonths(i);
                expensesByMonth.TryGetValue((month.Year, month.Month), out var total);
                return new ExpenseTrendDto(month.ToString("yyyy-MM"), total);
            })
            .ToList();

        return new ExpenseDashboardDto(
            todayTotal,
            weekTotal,
            monthTotal,
            yearTotal,
            byCategoryDtos,
            monthlyTrend);
    }
}
