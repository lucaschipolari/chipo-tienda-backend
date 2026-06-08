using ChipoBackend.Domain.Entities.Audit;
using ChipoBackend.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence.Repositories;

public class AuditLogRepository(AppDbContext context) : BaseRepository<AuditLog>(context), IAuditLogRepository
{
    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? entityName = null, Guid? userId = null,
        AuditAction? action = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var query = DbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityName)) query = query.Where(a => a.EntityName == entityName);
        if (userId.HasValue) query = query.Where(a => a.UserId == userId);
        if (action.HasValue) query = query.Where(a => a.Action == action);
        if (from.HasValue) query = query.Where(a => a.OccurredAt >= from);
        if (to.HasValue) query = query.Where(a => a.OccurredAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(a => a.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
