namespace ChipoBackend.Application.Features.Promotions.DTOs;

public record CouponListItemDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Type,
    decimal Value,
    string Currency,
    int UsedCount,
    int? UsageLimit,
    bool IsActive,
    bool IsStackable,
    DateTime? StartsAt,
    DateTime? EndsAt,
    DateTime CreatedAt);

public record CouponDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string Type,
    decimal Value,
    string Currency,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int UsedCount,
    int? UsageLimit,
    int? UsageLimitPerUser,
    bool IsActive,
    bool IsStackable,
    DateTime? StartsAt,
    DateTime? EndsAt,
    List<CouponRestrictionDto> Restrictions,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CouponRestrictionDto(string Type, Guid EntityId);

public record CouponUsageDto(
    Guid Id,
    Guid? CustomerId,
    Guid OrderId,
    decimal DiscountAmount,
    DateTime UsedAt);

public record ValidateCouponResultDto(
    bool IsValid,
    string? ErrorMessage,
    string? CouponType,
    decimal DiscountValue,
    decimal DiscountAmount,
    string Currency);
