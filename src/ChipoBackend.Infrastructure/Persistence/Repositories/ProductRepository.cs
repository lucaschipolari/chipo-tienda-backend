using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext context) : BaseRepository<Product>(context), IProductRepository
{
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await DbSet.Include(p => p.Variants).Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task<Product?> GetWithVariantsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(p => p.Variants).Include(p => p.Images).Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? categoryId = null, string? search = null,
        ProductStatus? status = null, CancellationToken ct = default)
    {
        var query = DbSet.Include(p => p.Variants).Include(p => p.Images).Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (status.HasValue) query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            // ILIKE → búsqueda insensible a mayúsculas ("PEPE" encuentra "pepe")
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, pattern) ||
                EF.Functions.ILike(p.Sku, pattern) ||
                p.Tags.Any(t => EF.Functions.ILike(t, pattern)));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default) =>
        await DbSet.AnyAsync(p => p.Slug == slug && (excludeId == null || p.Id != excludeId), ct);

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default) =>
        await DbSet.AnyAsync(p => p.Sku == sku && (excludeId == null || p.Id != excludeId), ct);
}
