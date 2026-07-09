using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class SupplierRepository(AppDbContext context) : BaseRepository<Supplier>(context), ISupplierRepository
{
    public async Task<Supplier?> GetByTaxIdAsync(string taxId, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(s => s.TaxId == taxId, ct);

    public async Task<Supplier?> GetWithContactsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(s => s.Contacts).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Supplier?> GetWithProductsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<(IReadOnlyList<Supplier> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            query = query.Where(sup =>
                sup.CompanyName.ToLower().Contains(s) ||
                (sup.TradeName != null && sup.TradeName.ToLower().Contains(s)) ||
                (sup.ContactName != null && sup.ContactName.ToLower().Contains(s)) ||
                (sup.TaxId != null && sup.TaxId.Contains(search)));
        }

        if (isActive.HasValue)
            query = query.Where(sup => sup.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(sup => sup.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
