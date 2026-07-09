using ChipoBackend.Domain.Entities.Orders;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<Order?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetForStatusUpdateAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Guid? customerId = null,
        OrderStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        string? search = null,
        string? email = null,
        string? buyerName = null,
        CancellationToken ct = default);
    Task<string> GenerateOrderNumberAsync(CancellationToken ct = default);
}
