namespace ChipoBackend.Application.Features.Inventory.DTOs;

public record StockMovementDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid VariantId,
    string VariantSku,
    Dictionary<string, string> VariantAttributes,
    string MovementType,
    int Quantity,
    int StockBefore,
    int StockAfter,
    string? Reason,
    string? ReferenceType,
    Guid? ReferenceId,
    Guid? CreatedByUserId,
    DateTime CreatedAt);

public record LowStockItemDto(
    Guid ProductId,
    string ProductName,
    string ProductSlug,
    Guid VariantId,
    string VariantSku,
    Dictionary<string, string> Attributes,
    int StockQuantity,
    int MinStockThreshold,
    int Deficit);

public record InventorySummaryDto(
    int TotalProducts,
    int TotalVariants,
    int LowStockCount,
    int OutOfStockCount,
    int TotalStockUnits);
