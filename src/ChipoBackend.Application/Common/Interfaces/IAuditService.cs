using ChipoBackend.Domain.Entities.Audit;

namespace ChipoBackend.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityName, string? entityId, string? oldValues = null, string? newValues = null, CancellationToken ct = default);
}
