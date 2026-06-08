using ChipoBackend.Domain.Entities.Purchasing;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default);
}
