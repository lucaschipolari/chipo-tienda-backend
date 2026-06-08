using ChipoBackend.Domain.Entities.Orders;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class OrderRepository(AppDbContext context) : BaseRepository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default) =>
        await DbSet.Include(o => o.Items).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<Order?> GetWithItemsAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(o => o.Items).Include(o => o.StatusHistory).Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? customerId = null, OrderStatus? status = null,
        DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        if (customerId.HasValue) query = query.Where(o => o.CustomerId == customerId);
        if (status.HasValue) query = query.Where(o => o.Status == status);
        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from);
        if (to.HasValue) query = query.Where(o => o.CreatedAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;
        var count = await DbSet.CountAsync(o => o.CreatedAt.Year == year, ct);
        return $"ORD-{year}-{(count + 1):D5}";
    }
}
