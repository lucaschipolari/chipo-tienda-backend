using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class StockMovementRepository(AppDbContext context) : BaseRepository<StockMovement>(context), IStockMovementRepository
{
    public async Task<IReadOnlyList<StockMovement>> GetByVariantAsync(Guid variantId, int limit = 50, CancellationToken ct = default) =>
        await DbSet.Where(m => m.VariantId == variantId)
            .OrderByDescending(m => m.CreatedAt).Take(limit).ToListAsync(ct);

    public async Task<(IReadOnlyList<StockMovement> Items, int TotalCount)> GetPagedAsync(
        Guid? productId, Guid? variantId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (productId.HasValue) query = query.Where(m => m.ProductId == productId.Value);
        if (variantId.HasValue) query = query.Where(m => m.VariantId == variantId.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<int> GetTotalStockUnitsAsync(CancellationToken ct = default)
    {
        // Suma total de stock activo en variantes de productos publicados
        return await context.Set<Domain.Entities.Catalog.ProductVariant>()
            .Where(v => v.IsActive)
            .SumAsync(v => v.StockQuantity, ct);
    }

    public async Task<int> CountLowStockVariantsAsync(CancellationToken ct = default)
    {
        return await context.Set<Domain.Entities.Catalog.ProductVariant>()
            .Where(v => v.IsActive && v.StockQuantity <= v.MinStockThreshold)
            .CountAsync(ct);
    }
}
