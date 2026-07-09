using ChipoBackend.Domain.Entities.Promotions;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IPromotionRepository : IRepository<Promotion>
{
    Task<(IReadOnlyList<Promotion> items, int total)> GetPagedAsync(
        int page, int pageSize,
        string? search, PromotionType? type, bool? isActive,
        CancellationToken ct = default);

    Task<Promotion?> GetWithRelationsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Promotion>> GetActiveValidAsync(CancellationToken ct = default);
}
