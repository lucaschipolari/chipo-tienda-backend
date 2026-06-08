using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class CouponRepository(AppDbContext context) : BaseRepository<Coupon>(context), ICouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

    public async Task<Coupon?> GetWithUsagesAsync(string code, CancellationToken ct = default) =>
        await DbSet.Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);
}
