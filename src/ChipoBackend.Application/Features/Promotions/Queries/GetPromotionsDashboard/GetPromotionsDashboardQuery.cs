using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetPromotionsDashboard;

public record GetPromotionsDashboardQuery() : IRequest<PromotionsDashboardDto>;

public class GetPromotionsDashboardQueryHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<GetPromotionsDashboardQuery, PromotionsDashboardDto>
{
    public async Task<PromotionsDashboardDto> Handle(GetPromotionsDashboardQuery request, CancellationToken cancellationToken)
    {
        var promotions = await promotionRepository.GetAllAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var totalActive = promotions.Count(p =>
            p.IsActive &&
            p.StartsAt <= now &&
            (p.EndsAt == null || p.EndsAt > now));

        var totalExpired = promotions.Count(p =>
            p.EndsAt.HasValue && p.EndsAt < now);

        var totalInactive = promotions.Count(p => !p.IsActive);

        var flashCount = promotions.Count(p => p.Type == PromotionType.Flash);
        var buyXGetYCount = promotions.Count(p => p.Type == PromotionType.BuyXGetY);
        var comboCount = promotions.Count(p => p.Type == PromotionType.Combo);

        var activePromotions = promotions
            .Where(p =>
                p.IsActive &&
                p.StartsAt <= now &&
                (p.EndsAt == null || p.EndsAt > now))
            .OrderByDescending(p => p.Priority)
            .Take(10)
            .Select(p => new PromotionSummaryDto(
                Id: p.Id,
                Name: p.Name,
                Type: p.Type.ToString(),
                Badge: p.Badge,
                EndsAt: p.EndsAt,
                IsStackable: p.IsStackable))
            .ToList();

        return new PromotionsDashboardDto(
            TotalActive: totalActive,
            TotalExpired: totalExpired,
            TotalInactive: totalInactive,
            FlashCount: flashCount,
            BuyXGetYCount: buyXGetYCount,
            ComboCount: comboCount,
            ActivePromotions: activePromotions);
    }
}
