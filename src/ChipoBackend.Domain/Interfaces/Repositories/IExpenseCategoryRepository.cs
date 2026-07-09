using ChipoBackend.Domain.Entities.Expenses;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
{
    Task<ExpenseCategory?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseCategory>> GetActiveAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<ExpenseCategory> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default);
}
