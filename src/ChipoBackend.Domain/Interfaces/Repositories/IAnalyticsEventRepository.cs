using ChipoBackend.Domain.Entities.Analytics;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public record ProductEventCount(Guid ProductId, int Count);
public record SearchTermCount(string Term, int Count, int NoResultCount);

public interface IAnalyticsEventRepository
{
    void Add(AnalyticsEvent e);

    Task<int> CountAsync(AnalyticsEventType type, DateTime from, DateTime to, CancellationToken ct = default);

    Task<int> DistinctProductsAsync(AnalyticsEventType type, DateTime from, DateTime to, CancellationToken ct = default);

    Task<int> DistinctSessionsAsync(DateTime from, DateTime to, CancellationToken ct = default);

    Task<IReadOnlyList<ProductEventCount>> TopProductsAsync(
        AnalyticsEventType type, DateTime from, DateTime to, int take, CancellationToken ct = default);

    Task<IReadOnlyList<SearchTermCount>> TopSearchesAsync(
        DateTime from, DateTime to, int take, bool noResultOnly, CancellationToken ct = default);
}
