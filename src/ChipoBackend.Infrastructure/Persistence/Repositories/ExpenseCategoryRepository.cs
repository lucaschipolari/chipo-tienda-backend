using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class ExpenseCategoryRepository(AppDbContext context)
    : BaseRepository<ExpenseCategory>(context), IExpenseCategoryRepository
{
    public async Task<ExpenseCategory?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLowerInvariant(), ct);

    public async Task<IReadOnlyList<ExpenseCategory>> GetActiveAsync(CancellationToken ct = default) =>
        await DbSet.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<(IReadOnlyList<ExpenseCategory> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                (c.Description != null && c.Description.ToLower().Contains(s)));
        }

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
