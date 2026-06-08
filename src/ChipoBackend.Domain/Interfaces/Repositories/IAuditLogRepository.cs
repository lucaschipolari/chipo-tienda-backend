using ChipoBackend.Domain.Entities.Audit;

namespace ChipoBackend.Domain.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? entityName = null,
        Guid? userId = null,
        AuditAction? action = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default);
}
