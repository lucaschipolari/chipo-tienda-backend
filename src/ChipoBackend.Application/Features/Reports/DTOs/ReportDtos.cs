namespace ChipoBackend.Application.Features.Reports.DTOs;

public record SalesReportDto(
    string Period,
    int TotalCount,
    decimal TotalRevenue,
    decimal TotalDiscount,
    decimal TotalTax,
    decimal AverageTicket,
    string Currency,
    List<SalesReportRowDto> Rows
);

public record SalesReportRowDto(
    DateTime Date,
    string SaleNumber,
    string BuyerName,
    string Channel,
    string PaymentMethod,
    int ItemCount,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal Total,
    string Currency
);

public record InventoryReportDto(
    List<InventoryReportRowDto> Rows,
    int TotalProducts,
    int OutOfStock,
    int Critical,
    decimal TotalValue
);

public record InventoryReportRowDto(
    Guid ProductId,
    string ProductName,
    string? VariantName,
    string Sku,
    string Category,
    int CurrentStock,
    int MinStock,
    decimal UnitCost,
    decimal UnitPrice,
    decimal TotalValue,
    string Status
);

public record PurchasesReportDto(
    int TotalCount,
    decimal TotalSpent,
    string Currency,
    List<PurchasesReportRowDto> Rows
);

public record PurchasesReportRowDto(
    string PurchaseNumber,
    string SupplierName,
    string Status,
    DateTime Date,
    int ItemCount,
    decimal Total,
    string Currency
);

public record ExpensesReportDto(
    int TotalCount,
    decimal TotalAmount,
    string Currency,
    List<ExpensesReportRowDto> Rows
);

public record ExpensesReportRowDto(
    DateTime Date,
    string Category,
    string Description,
    decimal Amount,
    string Currency,
    string Status
);

public record FinancialReportDto(
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal TotalPurchaseCosts,
    decimal NetProfit,
    string Currency,
    List<FinancialReportLineDto> Lines
);

public record FinancialReportLineDto(
    string Date,
    decimal Inflows,
    decimal Outflows,
    decimal Balance
);

public record ExportRequest(
    string ReportType,
    string Format,
    DateTime? From,
    DateTime? To,
    Guid? CategoryId,
    string? Status
);
