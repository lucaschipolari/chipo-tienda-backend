using ChipoBackend.Domain.Entities.Promotions;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IDiscountRepository : IRepository<Discount>
{
    Task<(IReadOnlyList<Discount> items, int total)> GetPagedAsync(
        int page, int pageSize,
        string? search, DiscountType? type, DiscountAppliesTo? appliesTo, bool? isActive,
        CancellationToken ct = default);
}
