namespace ChipoBackend.Application.Features.Promotions.DTOs;

public record PromotionListItemDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string? Badge,
    string DiscountType,
    decimal DiscountValue,
    string Currency,
    bool IsActive,
    bool IsStackable,
    int Priority,
    DateTime StartsAt,
    DateTime? EndsAt,
    DateTime CreatedAt);

public record PromotionDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    string? Badge,
    string DiscountType,
    decimal DiscountValue,
    string Currency,
    bool IsActive,
    bool IsStackable,
    int Priority,
    DateTime StartsAt,
    DateTime? EndsAt,
    string? ActiveFrom,
    string? ActiveUntil,
    int? BuyQuantity,
    int? GetQuantity,
    decimal? MinOrderAmount,
    decimal? ComboPrice,
    List<Guid> ProductIds,
    List<Guid> CategoryIds,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PromotionsDashboardDto(
    int TotalActive,
    int TotalExpired,
    int TotalInactive,
    int FlashCount,
    int BuyXGetYCount,
    int ComboCount,
    List<PromotionSummaryDto> ActivePromotions);

public record PromotionSummaryDto(
    Guid Id,
    string Name,
    string Type,
    string? Badge,
    DateTime? EndsAt,
    bool IsStackable);
