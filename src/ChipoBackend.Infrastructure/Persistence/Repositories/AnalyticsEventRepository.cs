using ChipoBackend.Domain.Entities.Analytics;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class AnalyticsEventRepository(AppDbContext context) : IAnalyticsEventRepository
{
    public void Add(AnalyticsEvent e) => context.AnalyticsEvents.Add(e);

    public Task<int> CountAsync(AnalyticsEventType type, DateTime from, DateTime to, CancellationToken ct = default) =>
        context.AnalyticsEvents.CountAsync(e => e.Type == type && e.CreatedAt >= from && e.CreatedAt <= to, ct);

    public Task<int> DistinctProductsAsync(AnalyticsEventType type, DateTime from, DateTime to, CancellationToken ct = default) =>
        context.AnalyticsEvents
            .Where(e => e.Type == type && e.ProductId != null && e.CreatedAt >= from && e.CreatedAt <= to)
            .Select(e => e.ProductId)
            .Distinct()
            .CountAsync(ct);

    public Task<int> DistinctSessionsAsync(DateTime from, DateTime to, CancellationToken ct = default) =>
        context.AnalyticsEvents
            .Where(e => e.SessionId != null && e.CreatedAt >= from && e.CreatedAt <= to)
            .Select(e => e.SessionId)
            .Distinct()
            .CountAsync(ct);

    public async Task<IReadOnlyList<ProductEventCount>> TopProductsAsync(
        AnalyticsEventType type, DateTime from, DateTime to, int take, CancellationToken ct = default)
    {
        return await context.AnalyticsEvents
            .Where(e => e.Type == type && e.ProductId != null && e.CreatedAt >= from && e.CreatedAt <= to)
            .GroupBy(e => e.ProductId!.Value)
            .Select(g => new ProductEventCount(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SearchTermCount>> TopSearchesAsync(
        DateTime from, DateTime to, int take, bool noResultOnly, CancellationToken ct = default)
    {
        var q = context.AnalyticsEvents
            .Where(e => e.Type == AnalyticsEventType.Search && e.SearchTerm != null
                     && e.CreatedAt >= from && e.CreatedAt <= to);
        if (noResultOnly)
            q = q.Where(e => e.ResultCount == 0);

        return await q
            .GroupBy(e => e.SearchTerm!)
            .Select(g => new SearchTermCount(
                g.Key,
                g.Count(),
                g.Count(x => x.ResultCount == 0)))
            .OrderByDescending(x => x.Count)
            .Take(take)
            .ToListAsync(ct);
    }
}
