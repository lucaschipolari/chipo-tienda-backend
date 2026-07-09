using ChipoBackend.Domain.Entities.Inventory;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IReadOnlyList<StockMovement>> GetByVariantAsync(Guid variantId, int limit = 50, CancellationToken ct = default);
    Task<(IReadOnlyList<StockMovement> Items, int TotalCount)> GetPagedAsync(
        Guid? productId, Guid? variantId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetTotalStockUnitsAsync(CancellationToken ct = default);
    Task<int> CountLowStockVariantsAsync(CancellationToken ct = default);
}
