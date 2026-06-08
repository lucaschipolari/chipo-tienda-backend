using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Audit;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;

namespace ChipoBackend.Infrastructure.Services;

public class AuditService(
    IAuditLogRepository auditLogRepository,
    ICurrentUserService currentUserService,
    IHttpContextAccessor httpContextAccessor,
    AppDbContext context) : IAuditService
{
    public async Task LogAsync(AuditAction action, string entityName, string? entityId, string? oldValues = null, string? newValues = null, CancellationToken ct = default)
    {
        var ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();

        var log = AuditLog.Create(
            currentUserService.UserId,
            currentUserService.Email,
            action,
            entityName,
            entityId,
            oldValues,
            newValues,
            ipAddress,
            userAgent);

        await auditLogRepository.AddAsync(log, ct);
        await context.SaveChangesAsync(ct);
    }
}
