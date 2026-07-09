using ChipoBackend.Application.Features.Finance.DTOs;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Finance.Queries.GetFinancialDashboard;

public record GetFinancialDashboardQuery : IRequest<FinancialDashboardDto>;

public class GetFinancialDashboardQueryHandler(
    ISaleRepository saleRepository,
    IExpenseRepository expenseRepository,
    IPurchaseOrderRepository purchaseOrderRepository
) : IRequestHandler<GetFinancialDashboardQuery, FinancialDashboardDto>
{
    public async Task<FinancialDashboardDto> Handle(GetFinancialDashboardQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
        var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(now.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

        // Sales this month
        var (salesMonth, _) = await saleRepository.GetPagedAsync(1, 1000, from: monthStart, to: monthEnd, ct: ct);
        var revenueMonth = salesMonth.Sum(s => s.Total.Amount);
        var transactionsMonth = salesMonth.Count;
        var averageTicket = transactionsMonth > 0 ? revenueMonth / transactionsMonth : 0m;

        // Sales this year
        var (salesYear, _) = await saleRepository.GetPagedAsync(1, 5000, from: yearStart, to: yearEnd, ct: ct);
        var revenueYear = salesYear.Sum(s => s.Total.Amount);

        // Expenses this month
        var expensesMonth = await expenseRepository.GetByDateRangeAsync(monthStart, monthEnd, ct);
        var totalExpensesMonth = expensesMonth.Sum(e => e.Amount.Amount);

        // Expenses this year
        var expensesYear = await expenseRepository.GetByDateRangeAsync(yearStart, yearEnd, ct);
        var totalExpensesYear = expensesYear.Sum(e => e.Amount.Amount);

        // Purchase costs this month (Received orders)
        var (purchasesMonth, _) = await purchaseOrderRepository.GetPagedAsync(1, 1000, status: nameof(PurchaseOrderStatus.Received), ct: ct);
        var purchasesInMonth = purchasesMonth.Where(p => p.UpdatedAt >= monthStart && p.UpdatedAt <= monthEnd).ToList();
        var totalPurchaseCostsMonth = purchasesInMonth.Sum(p => p.Total.Amount);

        // Purchase costs this year
        var purchasesInYear = purchasesMonth.Where(p => p.UpdatedAt >= yearStart && p.UpdatedAt <= yearEnd).ToList();
        var totalPurchaseCostsYear = purchasesInYear.Sum(p => p.Total.Amount);

        var grossProfitMonth = revenueMonth - totalPurchaseCostsMonth;
        var netProfitMonth = grossProfitMonth - totalExpensesMonth;
        var grossMarginMonth = revenueMonth > 0 ? Math.Round(grossProfitMonth / revenueMonth * 100, 2) : 0m;
        var netMarginMonth = revenueMonth > 0 ? Math.Round(netProfitMonth / revenueMonth * 100, 2) : 0m;

        // Last 12 months
        var last12Months = new List<FinancialPeriodDto>();
        for (int i = 11; i >= 0; i--)
        {
            var periodStart = monthStart.AddMonths(-i);
            var periodEnd = periodStart.AddMonths(1).AddTicks(-1);
            var label = periodStart.ToString("yyyy-MM");

            var (periodSales, _) = await saleRepository.GetPagedAsync(1, 1000, from: periodStart, to: periodEnd, ct: ct);
            var periodRevenue = periodSales.Sum(s => s.Total.Amount);

            var periodExpenses = await expenseRepository.GetByDateRangeAsync(periodStart, periodEnd, ct);
            var periodExpensesTotal = periodExpenses.Sum(e => e.Amount.Amount);

            var (periodPurchases, _) = await purchaseOrderRepository.GetPagedAsync(1, 1000, status: nameof(PurchaseOrderStatus.Received), ct: ct);
            var periodPurchasesTotal = periodPurchases
                .Where(p => p.UpdatedAt >= periodStart && p.UpdatedAt <= periodEnd)
                .Sum(p => p.Total.Amount);

            var periodProfit = periodRevenue - periodExpensesTotal - periodPurchasesTotal;
            var periodMargin = periodRevenue > 0 ? Math.Round(periodProfit / periodRevenue * 100, 2) : 0m;

            last12Months.Add(new FinancialPeriodDto(label, periodRevenue, periodExpensesTotal, periodPurchasesTotal, periodProfit, periodMargin));
        }

        // Top products from sales this month (in-memory grouping)
        var topProducts = salesMonth
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => new TopProductDto(
                ProductId: g.Key.ProductId,
                ProductName: g.Key.ProductName,
                UnitsSold: g.Sum(i => i.Quantity),
                Revenue: g.Sum(i => i.Total.Amount),
                EstimatedCost: 0m, // No cost data at sale item level
                GrossProfit: g.Sum(i => i.Total.Amount),
                Margin: 100m
            ))
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        // Cash flow last 30 days
        var cashFlowStart = now.Date.AddDays(-29).ToUniversalTime();
        var cashFlowEnd = now.Date.AddDays(1).ToUniversalTime().AddTicks(-1);

        var (cashFlowSales, _) = await saleRepository.GetPagedAsync(1, 1000, from: cashFlowStart, to: cashFlowEnd, ct: ct);
        var cashFlowExpenses = await expenseRepository.GetByDateRangeAsync(cashFlowStart, cashFlowEnd, ct);
        var (cashFlowPurchases, _) = await purchaseOrderRepository.GetPagedAsync(1, 1000, status: nameof(PurchaseOrderStatus.Received), ct: ct);
        var cashFlowPurchasesFiltered = cashFlowPurchases
            .Where(p => p.UpdatedAt >= cashFlowStart && p.UpdatedAt <= cashFlowEnd)
            .ToList();

        var cashFlowEntries = new List<CashFlowEntryDto>();
        decimal runningBalance = 0m;
        for (int d = 29; d >= 0; d--)
        {
            var day = now.Date.AddDays(-d);
            var dayStart = day.ToUniversalTime();
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);

            var dayInflows = cashFlowSales
                .Where(s => s.CreatedAt >= dayStart && s.CreatedAt <= dayEnd)
                .Sum(s => s.Total.Amount);

            var dayExpenseOutflows = cashFlowExpenses
                .Where(e => e.Date >= dayStart && e.Date <= dayEnd)
                .Sum(e => e.Amount.Amount);

            var dayPurchaseOutflows = cashFlowPurchasesFiltered
                .Where(p => p.UpdatedAt >= dayStart && p.UpdatedAt <= dayEnd)
                .Sum(p => p.Total.Amount);

            var dayOutflows = dayExpenseOutflows + dayPurchaseOutflows;
            var dayBalance = dayInflows - dayOutflows;
            runningBalance += dayBalance;

            cashFlowEntries.Add(new CashFlowEntryDto(
                Date: day.ToString("yyyy-MM-dd"),
                Inflows: dayInflows,
                Outflows: dayOutflows,
                Balance: dayBalance,
                RunningBalance: runningBalance
            ));
        }

        return new FinancialDashboardDto(
            TotalRevenueMonth: revenueMonth,
            TotalRevenueYear: revenueYear,
            TotalExpensesMonth: totalExpensesMonth,
            TotalExpensesYear: totalExpensesYear,
            TotalPurchaseCostsMonth: totalPurchaseCostsMonth,
            TotalPurchaseCostsYear: totalPurchaseCostsYear,
            GrossProfitMonth: grossProfitMonth,
            NetProfitMonth: netProfitMonth,
            GrossMarginMonth: grossMarginMonth,
            NetMarginMonth: netMarginMonth,
            AverageTicket: averageTicket,
            TotalTransactionsMonth: transactionsMonth,
            Last12Months: last12Months,
            TopProducts: topProducts,
            CashFlowLast30Days: cashFlowEntries
        );
    }
}
