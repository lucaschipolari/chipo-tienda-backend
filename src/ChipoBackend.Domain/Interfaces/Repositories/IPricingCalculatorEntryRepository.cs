using ChipoBackend.Domain.Entities.Catalog;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IPricingCalculatorEntryRepository
{
    Task<PricingCalculatorEntry?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Todas las entradas, indexadas por producto (para armar la calculadora en memoria).</summary>
    Task<Dictionary<Guid, PricingCalculatorEntry>> GetAllByProductAsync(CancellationToken ct = default);

    void Add(PricingCalculatorEntry entry);
}
