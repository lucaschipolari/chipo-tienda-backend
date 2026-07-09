using ChipoBackend.Domain.Entities.Expenses;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<Expense?> GetWithCategoryAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<Expense> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? categoryId = null,
        ExpenseStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        string? search = null,
        CancellationToken ct = default);

    Task<(IReadOnlyList<Expense> Expenses, IReadOnlyList<ExpenseCategory> Categories)> GetDashboardDataAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default);

    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default);
}
