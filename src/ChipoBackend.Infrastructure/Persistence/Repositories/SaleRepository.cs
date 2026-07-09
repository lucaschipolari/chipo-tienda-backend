using ChipoBackend.Domain.Entities.Sales;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class SaleRepository(AppDbContext context) : BaseRepository<Sale>(context), ISaleRepository
{
    public async Task<Sale?> GetWithItemsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? customerId = null,
        DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var query = DbSet.Include(s => s.Items).AsQueryable();

        if (customerId.HasValue) query = query.Where(s => s.CustomerId == customerId);
        if (from.HasValue) query = query.Where(s => s.CreatedAt >= from);
        if (to.HasValue) query = query.Where(s => s.CreatedAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<string> GenerateSaleNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await DbSet.CountAsync(s => s.CreatedAt.Year == year, ct);
        return $"VTA-{year}-{(count + 1):D5}";
    }

    public async Task<SalesSummaryData> GetSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var sales = await DbSet
            .Include(s => s.Items)
            .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
            .ToListAsync(ct);

        var totalRevenue = sales.Sum(s => s.Total.Amount);
        var totalSales = sales.Count;
        var avgTicket = totalSales > 0 ? totalRevenue / totalSales : 0;

        // Ventas por día
        var byDay = sales
            .GroupBy(s => s.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => (Date: g.Key, Revenue: g.Sum(s => s.Total.Amount), Count: g.Count()))
            .ToList();

        // Top productos
        var topProducts = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, i.ProductName })
            .Select(g => (
                ProductId: g.Key.ProductId,
                ProductName: g.Key.ProductName,
                Quantity: g.Sum(i => i.Quantity),
                Revenue: g.Sum(i => i.Total.Amount)))
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        // Top clientes (solo ventas con cliente asociado)
        var topCustomers = sales
            .Where(s => s.CustomerId.HasValue)
            .GroupBy(s => s.CustomerId!.Value)
            .Select(g => (
                CustomerId: g.Key,
                CustomerName: string.Empty, // Se enriquece en el handler
                Orders: g.Count(),
                Total: g.Sum(s => s.Total.Amount)))
            .OrderByDescending(x => x.Total)
            .Take(10)
            .ToList();

        return new SalesSummaryData(totalSales, totalRevenue, avgTicket, byDay, topProducts, topCustomers);
    }
}
