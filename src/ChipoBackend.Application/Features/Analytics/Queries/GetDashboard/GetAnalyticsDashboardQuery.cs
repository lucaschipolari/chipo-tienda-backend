using ChipoBackend.Application.Features.Analytics.DTOs;
using ChipoBackend.Domain.Entities.Analytics;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Analytics.Queries.GetDashboard;

public record GetAnalyticsDashboardQuery(DateTime From, DateTime To) : IRequest<AnalyticsDashboardDto>;

public class GetAnalyticsDashboardQueryHandler(
    IAnalyticsEventRepository analytics,
    IProductRepository products
) : IRequestHandler<GetAnalyticsDashboardQuery, AnalyticsDashboardDto>
{
    public async Task<AnalyticsDashboardDto> Handle(GetAnalyticsDashboardQuery request, CancellationToken ct)
    {
        var from = DateTime.SpecifyKind(request.From, DateTimeKind.Utc);
        var to = DateTime.SpecifyKind(request.To, DateTimeKind.Utc);

        // Período anterior equivalente (para la tendencia)
        var span = to - from;
        var prevFrom = from - span;
        var prevTo = from;

        var totalViews = await analytics.CountAsync(AnalyticsEventType.ProductView, from, to, ct);
        var totalCart = await analytics.CountAsync(AnalyticsEventType.AddToCart, from, to, ct);
        var totalFav = await analytics.CountAsync(AnalyticsEventType.Favorite, from, to, ct);
        var totalSearch = await analytics.CountAsync(AnalyticsEventType.Search, from, to, ct);
        var uniqueVisitors = await analytics.DistinctSessionsAsync(from, to, ct);
        var uniqueProducts = await analytics.DistinctProductsAsync(AnalyticsEventType.ProductView, from, to, ct);
        var prevViews = await analytics.CountAsync(AnalyticsEventType.ProductView, prevFrom, prevTo, ct);

        var viewToCart = totalViews > 0 ? Math.Round((double)totalCart / totalViews * 100, 1) : 0;
        var viewsTrend = prevViews > 0 ? Math.Round((double)(totalViews - prevViews) / prevViews * 100, 1) : 0;

        // Mapa de vistas por producto (para conversión y ranking de vistas)
        var viewCounts = await analytics.TopProductsAsync(AnalyticsEventType.ProductView, from, to, 1000, ct);
        var viewsMap = viewCounts.ToDictionary(v => v.ProductId, v => v.Count);

        var cartCounts = await analytics.TopProductsAsync(AnalyticsEventType.AddToCart, from, to, 10, ct);
        var favCounts = await analytics.TopProductsAsync(AnalyticsEventType.Favorite, from, to, 10, ct);

        // Nombres de producto
        var ids = viewCounts.Take(10).Select(v => v.ProductId)
            .Concat(cartCounts.Select(c => c.ProductId))
            .Concat(favCounts.Select(f => f.ProductId))
            .Distinct().ToList();
        var names = new Dictionary<Guid, (string Name, string? Cat)>();
        foreach (var id in ids)
        {
            var p = await products.GetByIdAsync(id, ct);
            if (p != null) names[id] = (p.Name, p.Category?.Name);
        }

        ProductStatDto Map(ProductEventCount c, bool withConversion)
        {
            var info = names.TryGetValue(c.ProductId, out var n) ? n : (Name: "(eliminado)", Cat: (string?)null);
            var views = viewsMap.GetValueOrDefault(c.ProductId, 0);
            double? conv = withConversion && views > 0 ? Math.Round((double)c.Count / views * 100, 1) : null;
            return new ProductStatDto(c.ProductId, info.Name, info.Cat, c.Count, views, conv);
        }

        var topViewed = viewCounts.Take(10).Select(c => Map(c, false)).ToList();
        var topCart = cartCounts.Select(c => Map(c, true)).ToList();
        var topFav = favCounts.Select(c => Map(c, false)).ToList();

        var topSearches = (await analytics.TopSearchesAsync(from, to, 15, false, ct))
            .Select(s => new SearchStatDto(s.Term, s.Count, s.NoResultCount)).ToList();
        var noResult = (await analytics.TopSearchesAsync(from, to, 15, true, ct))
            .Select(s => new SearchStatDto(s.Term, s.Count, s.NoResultCount)).ToList();

        var summary = new AnalyticsSummaryDto(
            totalViews, totalCart, totalFav, totalSearch,
            uniqueVisitors, uniqueProducts, viewToCart, viewsTrend);

        return new AnalyticsDashboardDto(from, to, summary, topViewed, topCart, topFav, topSearches, noResult);
    }
}
