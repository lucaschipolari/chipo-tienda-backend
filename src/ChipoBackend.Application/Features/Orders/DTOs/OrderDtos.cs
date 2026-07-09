namespace ChipoBackend.Application.Features.Orders.DTOs;

public record OrderAddressDto(
    string Street,
    string City,
    string? State,
    string? PostalCode,
    string Country
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantDescription,
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total,
    string Currency
);

public record OrderStatusHistoryDto(
    string? FromStatus,
    string ToStatus,
    string? Note,
    Guid? ChangedByUserId,
    DateTime ChangedAt
);

public record PaymentDto(
    Guid Id,
    string Method,
    decimal Amount,
    string Currency,
    string Status,
    string? GatewayRef,
    DateTime? ProcessedAt,
    DateTime CreatedAt
);

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid? CustomerId,
    string? CustomerName,
    string? CustomerEmail,
    string BuyerName,
    string BuyerEmail,
    string? BuyerPhone,
    string? PaymentMethod,
    string? DeliveryMethod,
    string Status,
    OrderAddressDto ShippingAddress,
    OrderAddressDto? BillingAddress,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal ShippingCost,
    decimal TaxAmount,
    decimal Total,
    string Currency,
    string? CouponCode,
    string? Notes,
    string? CancelReason,
    DateTime? PaidAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    List<OrderItemDto> Items,
    List<OrderStatusHistoryDto> StatusHistory,
    List<PaymentDto> Payments,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record OrderListItemDto(
    Guid Id,
    string OrderNumber,
    Guid? CustomerId,
    string? CustomerName,
    string BuyerName,
    string BuyerEmail,
    string Status,
    int ItemCount,
    decimal Total,
    string Currency,
    DateTime? PaidAt,
    DateTime CreatedAt
);

// Request bodies
public record CreateOrderItemRequest(
    Guid ProductId,
    Guid VariantId,
    int Quantity,
    decimal? UnitPriceOverride = null,
    decimal Discount = 0
);

public record CreateOrderAddressRequest(
    string Street,
    string City,
    string? State,
    string? PostalCode,
    string Country = "Argentina"
);

/// <summary>
/// Alias for guest checkout — same fields as CreateOrderCommand, used as a named request body.
/// </summary>
public record CreateGuestOrderRequest(
    string BuyerName,
    string BuyerEmail,
    string? BuyerPhone,
    List<CreateOrderItemRequest> Items,
    CreateOrderAddressRequest ShippingAddress,
    CreateOrderAddressRequest? BillingAddress,
    decimal ShippingCost = 0,
    string Currency = "ARS",
    string? PaymentMethod = null,
    string? DeliveryMethod = null,
    string? CouponCode = null,
    string? Notes = null
);
