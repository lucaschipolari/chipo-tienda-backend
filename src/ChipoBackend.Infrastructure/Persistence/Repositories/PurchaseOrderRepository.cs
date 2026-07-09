using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class PurchaseOrderRepository(AppDbContext context) : BaseRepository<PurchaseOrder>(context), IPurchaseOrderRepository
{
    public async Task<PurchaseOrder?> GetWithItemsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? supplierId = null,
        string? status = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Include(p => p.Items).AsQueryable();

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseOrderStatus>(status, true, out var parsedStatus))
            query = query.Where(p => p.Status == parsedStatus);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<string> GeneratePurchaseNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await DbSet.CountAsync(p => p.CreatedAt.Year == year, ct);
        return $"OC-{year}-{(count + 1):D5}";
    }
}
