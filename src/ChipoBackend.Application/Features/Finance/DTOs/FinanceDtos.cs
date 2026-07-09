namespace ChipoBackend.Application.Features.Finance.DTOs;

// ── New Finance Dashboard DTOs ────────────────────────────────────────────────

public record FinancialKpiDto(
    decimal TotalRevenue,
    decimal TotalCosts,
    decimal TotalExpenses,
    decimal GrossProfit,
    decimal NetProfit,
    decimal GrossMargin,
    decimal NetMargin,
    decimal AverageTicket,
    int TotalSales,
    decimal RevenueVsPreviousPeriod,
    string Currency);

/// <summary>New cash-flow DTO used by GetCashFlowQuery and GetFinanceDashboardQuery.</summary>
public record CashFlowDto(
    List<CashFlowEntryDto> Entries,
    decimal TotalInflows,
    decimal TotalOutflows,
    decimal NetBalance,
    string Currency);

public record RevenueByDayDto(
    string Date,
    decimal Revenue,
    decimal Costs,
    decimal Expenses);

/// <summary>Top-product DTO used by GetFinanceDashboardQuery (finance-focused fields).</summary>
public record FinanceTopProductDto(
    string ProductName,
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    decimal Margin,
    int Quantity);

public record FinanceDashboardDto(
    FinancialKpiDto Kpis,
    CashFlowDto CashFlow,
    List<RevenueByDayDto> RevenueByDay,
    List<FinanceTopProductDto> TopProducts,
    string Period,
    DateTime From,
    DateTime To);

// ─────────────────────────────────────────────────────────────────────────────

public record FinancialPeriodDto(
    string Period,
    decimal Income,
    decimal Expenses,
    decimal PurchaseCosts,
    decimal Profit,
    decimal Margin
);

public record CashFlowEntryDto(
    string Date,
    decimal Inflows,
    decimal Outflows,
    decimal Balance,
    decimal RunningBalance
);

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    int UnitsSold,
    decimal Revenue,
    decimal EstimatedCost,
    decimal GrossProfit,
    decimal Margin
);

public record FinancialDashboardDto(
    decimal TotalRevenueMonth,
    decimal TotalRevenueYear,
    decimal TotalExpensesMonth,
    decimal TotalExpensesYear,
    decimal TotalPurchaseCostsMonth,
    decimal TotalPurchaseCostsYear,
    decimal GrossProfitMonth,
    decimal NetProfitMonth,
    decimal GrossMarginMonth,
    decimal NetMarginMonth,
    decimal AverageTicket,
    int TotalTransactionsMonth,
    List<FinancialPeriodDto> Last12Months,
    List<TopProductDto> TopProducts,
    List<CashFlowEntryDto> CashFlowLast30Days
);

public record ExecutiveSummaryDto(
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal TotalPurchaseCosts,
    decimal GrossProfit,
    decimal NetProfit,
    decimal OverallMargin,
    decimal AverageMonthlyRevenue,
    decimal AverageMonthlyExpenses,
    List<FinancialPeriodDto> ByPeriod,
    List<TopProductDto> TopProducts,
    List<TopProductDto> BottomProducts
);

public record CashFlowReportDto(
    decimal TotalInflows,
    decimal TotalOutflows,
    decimal NetCashFlow,
    List<CashFlowEntryDto> Entries
);
