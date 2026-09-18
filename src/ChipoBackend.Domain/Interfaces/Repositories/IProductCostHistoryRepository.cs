using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IProductCostHistoryRepository : IRepository<ProductCostHistory>
{
    /// <summary>Historial de costos de una variante, del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<ProductCostHistory>> GetByVariantAsync(Guid variantId, CancellationToken ct = default);

    /// <summary>Historial de costos de todas las variantes de un producto (más reciente primero).</summary>
    Task<IReadOnlyList<ProductCostHistory>> GetByProductAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Todo el historial (para armar la tabla general en memoria).</summary>
    Task<IReadOnlyList<ProductCostHistory>> GetAllOrderedAsync(CancellationToken ct = default);
}
