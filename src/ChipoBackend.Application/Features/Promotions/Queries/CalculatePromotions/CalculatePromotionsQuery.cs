using ChipoBackend.Application.Common.Interfaces;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.CalculatePromotions;

public record CalculatePromotionsQuery(
    List<PromotionCartItem> Items,
    decimal SubTotal,
    string Currency,
    string? CouponCode,
    Guid? CustomerId) : IRequest<PromotionCalculationResult>;

public class CalculatePromotionsQueryHandler : IRequestHandler<CalculatePromotionsQuery, PromotionCalculationResult>
{
    private readonly IPromotionEngine _promotionEngine;

    public CalculatePromotionsQueryHandler(IPromotionEngine promotionEngine)
    {
        _promotionEngine = promotionEngine;
    }

    public async Task<PromotionCalculationResult> Handle(CalculatePromotionsQuery request, CancellationToken cancellationToken)
    {
        return await _promotionEngine.CalculateAsync(
            new PromotionCalculationRequest(
                request.Items,
                request.SubTotal,
                request.Currency,
                request.CouponCode,
                request.CustomerId),
            cancellationToken);
    }
}
