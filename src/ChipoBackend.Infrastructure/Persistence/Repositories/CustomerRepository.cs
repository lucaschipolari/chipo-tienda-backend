using ChipoBackend.Domain.Entities.Customers;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class CustomerRepository(AppDbContext context) : BaseRepository<Customer>(context), ICustomerRepository
{
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), ct);

    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await DbSet.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task<Customer?> GetWithAddressesAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Customer?> GetByDocumentNumberAsync(string documentNumber, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber, ct);

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(s) ||
                c.LastName.ToLower().Contains(s) ||
                (c.Email != null && c.Email.Contains(s)) ||
                (c.PhoneNumber != null && c.PhoneNumber.Contains(s)) ||
                c.DocumentNumber.Contains(s));
        }

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.FirstName).ThenBy(c => c.LastName)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
