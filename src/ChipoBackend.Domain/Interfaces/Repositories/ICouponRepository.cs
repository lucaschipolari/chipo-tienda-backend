using ChipoBackend.Domain.Entities.Promotions;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface ICouponRepository : IRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Coupon?> GetWithUsagesAsync(string code, CancellationToken ct = default);
}
