using ChipoBackend.Domain.Entities.Sales;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Sale> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Guid? customerId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);
    Task<string> GenerateSaleNumberAsync(CancellationToken ct = default);
    Task<SalesSummaryData> GetSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public record SalesSummaryData(
    int TotalSales,
    decimal TotalRevenue,
    decimal AverageTicket,
    List<(DateTime Date, decimal Revenue, int Count)> ByDay,
    List<(Guid ProductId, string ProductName, int Quantity, decimal Revenue)> TopProducts,
    List<(Guid CustomerId, string CustomerName, int Orders, decimal Total)> TopCustomers
);
