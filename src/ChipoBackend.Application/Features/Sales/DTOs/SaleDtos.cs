namespace ChipoBackend.Application.Features.Sales.DTOs;

public record SaleItemDto(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total,
    string Currency
);

public record SaleDto(
    Guid Id,
    string SaleNumber,
    Guid? CustomerId,
    string? CustomerName,
    Guid SoldByUserId,
    string? SoldByUserName,
    string Channel,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string Currency,
    string PaymentMethod,
    string? Notes,
    List<SaleItemDto> Items,
    DateTime CreatedAt
);

public record SaleListItemDto(
    Guid Id,
    string SaleNumber,
    Guid? CustomerId,
    string? CustomerName,
    string Channel,
    int ItemCount,
    decimal Total,
    string Currency,
    string PaymentMethod,
    DateTime CreatedAt
);

// Reportes
public record DailyRevenueDto(DateTime Date, decimal Revenue, int SalesCount);

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    int TotalQuantity,
    decimal TotalRevenue
);

public record TopCustomerDto(
    Guid CustomerId,
    string CustomerName,
    int TotalOrders,
    decimal TotalSpent
);

public record SalesReportDto(
    DateTime From,
    DateTime To,
    int TotalSales,
    decimal TotalRevenue,
    decimal AverageTicket,
    decimal RevenueVsPreviousPeriod,   // % de variación
    List<DailyRevenueDto> ByDay,
    List<TopProductDto> TopProducts,
    List<TopCustomerDto> TopCustomers
);

// Request body para crear venta
public record CreateSaleItemRequest(
    Guid ProductId,
    Guid VariantId,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0
);
