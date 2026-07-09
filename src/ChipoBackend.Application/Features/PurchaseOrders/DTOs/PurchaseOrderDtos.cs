namespace ChipoBackend.Application.Features.PurchaseOrders.DTOs;

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

public record PurchaseOrderItemDto(
    Guid Id,
    Guid ProductId,
    string? ProductName,
    Guid VariantId,
    string? VariantSku,
    int Quantity,
    int QuantityReceived,
    int PendingQuantity,
    bool IsFullyReceived,
    decimal UnitCost,
    decimal Total,
    string Currency
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

public record CreatePurchaseOrderItemRequest(
    Guid ProductId,
    Guid VariantId,
    int Quantity,
    decimal UnitCost,
    string Currency = "ARS"
);

public record ReceiveItemsRequest(Dictionary<Guid, int> ItemReceipts);
