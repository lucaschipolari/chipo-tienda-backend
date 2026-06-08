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

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.FirstName.Contains(search) || c.LastName.Contains(search) || (c.Email != null && c.Email.Contains(search)));

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(c => c.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
