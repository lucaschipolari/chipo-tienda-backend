using ChipoBackend.Domain.Entities.Purchasing;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByTaxIdAsync(string taxId, CancellationToken ct = default);
    Task<Supplier?> GetWithContactsAsync(Guid id, CancellationToken ct = default);
    Task<Supplier?> GetWithProductsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default);
}
