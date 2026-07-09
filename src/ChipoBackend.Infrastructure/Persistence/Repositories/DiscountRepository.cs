using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class DiscountRepository(AppDbContext context) : BaseRepository<Discount>(context), IDiscountRepository
{
    public async Task<(IReadOnlyList<Discount> items, int total)> GetPagedAsync(
        int page, int pageSize, string? search, DiscountType? type, DiscountAppliesTo? appliesTo, bool? isActive,
        CancellationToken ct = default)
    {
        var q = DbSet.Where(d => !d.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(d => d.Name.Contains(search) || (d.Description != null && d.Description.Contains(search)));
        if (type.HasValue) q = q.Where(d => d.Type == type.Value);
        if (appliesTo.HasValue) q = q.Where(d => d.AppliesTo == appliesTo.Value);
        if (isActive.HasValue) q = q.Where(d => d.IsActive == isActive.Value);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(d => d.Priority).ThenByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
