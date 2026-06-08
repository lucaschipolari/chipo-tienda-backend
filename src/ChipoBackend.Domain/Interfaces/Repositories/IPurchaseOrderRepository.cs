using ChipoBackend.Domain.Entities.Purchasing;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
    Task<PurchaseOrder?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<PurchaseOrder> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, Guid? supplierId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default);
    Task<string> GeneratePurchaseNumberAsync(CancellationToken ct = default);
}
