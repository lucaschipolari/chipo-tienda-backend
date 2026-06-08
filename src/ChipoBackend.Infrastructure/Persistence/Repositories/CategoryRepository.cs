using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class CategoryRepository(AppDbContext context) : BaseRepository<Category>(context), ICategoryRepository
{
    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default) =>
        await DbSet.Where(c => c.IsActive).Include(c => c.SubCategories)
            .OrderBy(c => c.DisplayOrder).ToListAsync(ct);

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default) =>
        await DbSet.AnyAsync(c => c.Slug == slug && (excludeId == null || c.Id != excludeId), ct);
}
