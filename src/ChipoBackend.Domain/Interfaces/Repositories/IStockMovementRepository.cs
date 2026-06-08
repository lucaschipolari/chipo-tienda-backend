using ChipoBackend.Domain.Entities.Inventory;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IReadOnlyList<StockMovement>> GetByVariantAsync(Guid variantId, int limit = 50, CancellationToken ct = default);
}
