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

    // Carga el pedido SIN StatusHistory para evitar que EF confunda la nueva
    // entrada de historial (Added) con la existente (Unchanged) y genere un
    // UPDATE en lugar de un INSERT → DbUpdateConcurrencyException.
    public async Task<Order?> GetForStatusUpdateAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.Include(o => o.Items).Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<Order?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await DbSet.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.BuyerEmail == email, ct);

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? customerId = null, OrderStatus? status = null,
        DateTime? from = null, DateTime? to = null, string? search = null,
        string? email = null, string? buyerName = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Include(o => o.Items).AsQueryable();

        if (customerId.HasValue) query = query.Where(o => o.CustomerId == customerId);
        if (status.HasValue) query = query.Where(o => o.Status == status);
        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from);
        if (to.HasValue) query = query.Where(o => o.CreatedAt <= to);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(search.ToLower()) ||
                o.BuyerName.ToLower().Contains(search.ToLower()) ||
                o.BuyerEmail.ToLower().Contains(search.ToLower()));
        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(o => o.BuyerEmail.Contains(email));
        if (!string.IsNullOrWhiteSpace(buyerName))
            query = query.Where(o => o.BuyerName.Contains(buyerName));

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
