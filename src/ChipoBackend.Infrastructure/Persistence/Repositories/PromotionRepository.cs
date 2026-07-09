using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class PromotionRepository(AppDbContext context) : BaseRepository<Promotion>(context), IPromotionRepository
{
    public async Task<(IReadOnlyList<Promotion> items, int total)> GetPagedAsync(
        int page, int pageSize, string? search, PromotionType? type, bool? isActive,
        CancellationToken ct = default)
    {
        var q = DbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
        if (type.HasValue) q = q.Where(p => p.Type == type.Value);
        if (isActive.HasValue) q = q.Where(p => p.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var items = await q.Include(p => p.Products).Include(p => p.Categories)
            .OrderByDescending(p => p.Priority).ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Promotion?> GetWithRelationsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(p => p.Products).Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Promotion>> GetActiveValidAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet.Include(p => p.Products).Include(p => p.Categories)
            .Where(p => p.IsActive && p.StartsAt <= now && (p.EndsAt == null || p.EndsAt >= now))
            .OrderByDescending(p => p.Priority)
            .ToListAsync(ct);
    }
}
