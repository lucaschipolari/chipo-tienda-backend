using ChipoBackend.Domain.Entities.Customers;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken ct = default);
    Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default);
}
