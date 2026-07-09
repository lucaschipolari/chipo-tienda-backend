using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class ExpenseRepository(AppDbContext context)
    : BaseRepository<Expense>(context), IExpenseRepository
{
    public async Task<Expense?> GetWithCategoryAsync(Guid id, CancellationToken ct = default) =>
        await DbSet
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<(IReadOnlyList<Expense> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? categoryId = null,
        ExpenseStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = DbSet.Include(e => e.Category).AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value.Date);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            query = query.Where(e =>
                e.Description.ToLower().Contains(s) ||
                (e.Observations != null && e.Observations.ToLower().Contains(s)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(IReadOnlyList<Expense> Expenses, IReadOnlyList<ExpenseCategory> Categories)> GetDashboardDataAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        var expenses = await DbSet
            .Include(e => e.Category)
            .Where(e => e.Date >= from.Date && e.Date <= to.Date && e.Status != ExpenseStatus.Cancelled)
            .OrderByDescending(e => e.Date)
            .ToListAsync(ct);

        var categoryIds = expenses.Select(e => e.CategoryId).Distinct().ToList();
        var categories = await context.Set<ExpenseCategory>()
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(ct);

        return (expenses, categories);
    }

    public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default) =>
        await DbSet
            .Include(e => e.Category)
            .Where(e => e.Date >= from.Date && e.Date <= to.Date)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);
}
