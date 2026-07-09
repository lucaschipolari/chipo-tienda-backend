using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class CouponRepository(AppDbContext context) : BaseRepository<Coupon>(context), ICouponRepository
{
    public async Task<(IReadOnlyList<Coupon> items, int total)> GetPagedAsync(
        int page, int pageSize, string? search, CouponType? type, bool? isActive,
        CancellationToken ct = default)
    {
        var q = DbSet.Where(c => !c.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.Code.Contains(search) || c.Name.Contains(search) ||
                (c.Description != null && c.Description.Contains(search)));
        if (type.HasValue) q = q.Where(c => c.Type == type.Value);
        if (isActive.HasValue) q = q.Where(c => c.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant() && !c.IsDeleted, ct);

    public async Task<Coupon?> GetWithUsagesAsync(string code, CancellationToken ct = default)
        => await DbSet.Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant() && !c.IsDeleted, ct);

    public async Task<Coupon?> GetWithUsagesByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

    public async Task<Coupon?> GetWithUsagesByCodeAsync(string code, CancellationToken ct = default)
        => await DbSet.Include(c => c.Usages)
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant() && !c.IsDeleted, ct);

    public async Task<Coupon?> GetWithRestrictionsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(c => c.Restrictions)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => await DbSet.AnyAsync(c => c.Code == code.ToUpperInvariant() && !c.IsDeleted
            && (excludeId == null || c.Id != excludeId.Value), ct);
}
