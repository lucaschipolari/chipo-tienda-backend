using ChipoBackend.Domain.Entities.Combos;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class ComboRepository(AppDbContext context) : BaseRepository<Combo>(context), IComboRepository
{
    public async Task<Combo?> GetWithItemsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Combo>> GetAllWithItemsAsync(bool onlyActive, CancellationToken ct = default)
    {
        var q = DbSet.Include(c => c.Items).AsQueryable();
        if (onlyActive) q = q.Where(c => c.IsActive);
        return await q.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId, CancellationToken ct = default) =>
        await DbSet.AnyAsync(c => c.Slug == slug && (excludeId == null || c.Id != excludeId), ct);
}
