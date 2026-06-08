using System.Text.Json;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ChipoBackend.Infrastructure.Persistence.Interceptors;

public class AuditSaveChangesInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        var userId = currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                ((dynamic)entry.Entity).CreatedAt = now;
                ((dynamic)entry.Entity).UpdatedAt = now;
                ((dynamic)entry.Entity).CreatedByUserId = userId;
                ((dynamic)entry.Entity).UpdatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                ((dynamic)entry.Entity).UpdatedAt = now;
                ((dynamic)entry.Entity).UpdatedByUserId = userId;
            }
        }

        var auditEntries = new List<AuditLog>();
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog) continue;

            AuditAction? action = entry.State switch
            {
                EntityState.Added => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted => AuditAction.Delete,
                _ => null
            };

            if (action == null) continue;

            string? oldValues = null;
            string? newValues = null;

            if (action == AuditAction.Update || action == AuditAction.Delete)
                oldValues = JsonSerializer.Serialize(entry.OriginalValues.Properties
                    .ToDictionary(p => p.Name, p => entry.OriginalValues[p]));

            if (action == AuditAction.Create || action == AuditAction.Update)
                newValues = JsonSerializer.Serialize(entry.CurrentValues.Properties
                    .ToDictionary(p => p.Name, p => entry.CurrentValues[p]));

            auditEntries.Add(AuditLog.Create(
                userId, currentUserService.Email,
                action.Value,
                entry.Entity.GetType().Name,
                entry.Entity.Id.ToString(),
                oldValues, newValues));
        }

        if (auditEntries.Count > 0)
            context.Set<AuditLog>().AddRange(auditEntries);
    }
}
