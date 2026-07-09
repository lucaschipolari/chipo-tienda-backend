using ChipoBackend.Domain.Entities.Promotions;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<(IReadOnlyList<Coupon> items, int total)> GetPagedAsync(
        int page, int pageSize,
        string? search, CouponType? type, bool? isActive,
        CancellationToken ct = default);

    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Coupon?> GetWithUsagesAsync(string code, CancellationToken ct = default);
    Task<Coupon?> GetWithUsagesByIdAsync(Guid id, CancellationToken ct = default);
    Task<Coupon?> GetWithUsagesByCodeAsync(string code, CancellationToken ct = default);
    Task<Coupon?> GetWithRestrictionsAsync(Guid id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
}
