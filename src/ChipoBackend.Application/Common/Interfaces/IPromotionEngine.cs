namespace ChipoBackend.Application.Common.Interfaces;

public interface IPromotionEngine
{
    Task<PromotionCalculationResult> CalculateAsync(PromotionCalculationRequest request, CancellationToken ct = default);
}

public record PromotionCalculationRequest(
    List<PromotionCartItem> Items,
    decimal SubTotal,
    string Currency,
    string? CouponCode,
    Guid? CustomerId);

public record PromotionCartItem(
    Guid ProductId,
    Guid? CategoryId,
    int Quantity,
    decimal UnitPrice,
    string ProductName);

public record PromotionCalculationResult(
    decimal OriginalTotal,
    decimal PromotionDiscountAmount,
    decimal CouponDiscountAmount,
    decimal FinalTotal,
    decimal TotalSavings,
    List<AppliedPromotionDto> AppliedPromotions,
    AppliedCouponDto? AppliedCoupon,
    bool IsValid,
    List<string> Messages);

public record AppliedPromotionDto(Guid Id, string Name, string? Badge, decimal SavedAmount);

public record AppliedCouponDto(string Code, string Type, decimal SavedAmount);
