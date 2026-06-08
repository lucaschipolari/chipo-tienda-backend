using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class StockMovementRepository(AppDbContext context) : BaseRepository<StockMovement>(context), IStockMovementRepository
{
    public async Task<IReadOnlyList<StockMovement>> GetByVariantAsync(Guid variantId, int limit = 50, CancellationToken ct = default) =>
        await DbSet.Where(m => m.VariantId == variantId)
            .OrderByDescending(m => m.CreatedAt).Take(limit).ToListAsync(ct);
}
