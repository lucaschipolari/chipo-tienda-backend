using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class ProductCostHistoryRepository(AppDbContext context)
    : BaseRepository<ProductCostHistory>(context), IProductCostHistoryRepository
{
    public async Task<IReadOnlyList<ProductCostHistory>> GetByVariantAsync(Guid variantId, CancellationToken ct = default) =>
        await DbSet.Where(h => h.VariantId == variantId)
            .OrderByDescending(h => h.RecordedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProductCostHistory>> GetByProductAsync(Guid productId, CancellationToken ct = default) =>
        await DbSet.Where(h => h.ProductId == productId)
            .OrderByDescending(h => h.RecordedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProductCostHistory>> GetAllOrderedAsync(CancellationToken ct = default) =>
        await DbSet.OrderByDescending(h => h.RecordedAt).ToListAsync(ct);
}
