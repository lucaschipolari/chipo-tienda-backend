namespace ChipoBackend.Application.Features.Purchases.DTOs;

public record PurchaseOrderItemDto(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    Guid VariantId,
    string? VariantSku,
    int Quantity,
    int QuantityReceived,
    decimal UnitCost,
    decimal Total,
    string Currency,
    bool IsFullyReceived
);

public record PurchaseOrderDto(
    Guid Id,
    string PurchaseNumber,
    Guid SupplierId,
    string? SupplierName,
    string Status,
    DateTime? ExpectedDeliveryDate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    string Currency,
    string? Notes,
    List<PurchaseOrderItemDto> Items,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PurchaseOrderListItemDto(
    Guid Id,
    string PurchaseNumber,
    Guid SupplierId,
    string? SupplierName,
    string Status,
    int ItemCount,
    decimal Total,
    string Currency,
    DateTime? ExpectedDeliveryDate,
    DateTime CreatedAt
);

public record CreatePurchaseOrderItemRequest(
    Guid ProductId,
    Guid VariantId,
    int Quantity,
    decimal UnitCost
);

public record CreatePurchaseOrderRequest(
    Guid SupplierId,
    DateTime? ExpectedDeliveryDate,
    string Currency,
    string? Notes,
    List<CreatePurchaseOrderItemRequest> Items
);

public record ReceivePurchaseOrderRequest(
    Dictionary<Guid, int> ItemReceipts
);
