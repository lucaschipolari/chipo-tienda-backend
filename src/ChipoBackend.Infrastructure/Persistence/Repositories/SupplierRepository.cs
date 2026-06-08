using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class SupplierRepository(AppDbContext context) : BaseRepository<Supplier>(context), ISupplierRepository
{
    public async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.CompanyName.Contains(search) || (s.ContactName != null && s.ContactName.Contains(search)));

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(s => s.CompanyName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
