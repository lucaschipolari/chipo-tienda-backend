using System.Text.Json;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ChipoBackend.Infrastructure.Persistence.Interceptors;

public class AuditSaveChangesInterceptor(
    ICurrentUserService currentUserService,
    ILogger<AuditSaveChangesInterceptor> logger) : SaveChangesInterceptor
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

    // Captura la excepción de concurrencia y loguea exactamente qué entidad falla.
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        LogConcurrencyError(eventData);
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken ct = default)
    {
        LogConcurrencyError(eventData);
        return base.SaveChangesFailedAsync(eventData, ct);
    }

    private void LogConcurrencyError(DbContextErrorEventData eventData)
    {
        if (eventData.Exception is not DbUpdateConcurrencyException concEx) return;

        logger.LogError("=== DbUpdateConcurrencyException ===");
        foreach (var entry in concEx.Entries)
        {
            logger.LogError(
                "  Entidad: {Type} | Estado: {State} | IsOwned: {IsOwned} | Id: {Id}",
                entry.Entity.GetType().Name,
                entry.State,
                entry.Metadata.IsOwned(),
                entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue ?? "N/A");

            foreach (var prop in entry.Properties)
            {
                logger.LogError(
                    "    Prop: {Name} | Original: {Original} | Current: {Current} | IsModified: {IsModified}",
                    prop.Metadata.Name, prop.OriginalValue, prop.CurrentValue, prop.IsModified);
            }
        }
    }

    private void UpdateAuditableEntities(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        var userId = currentUserService.UserId;

        // Materialize to list BEFORE modifying anything to avoid EF Core enumeration issues.
        // Skip owned entities (e.g. EmailAddress OwnsOne) — they share the owner's table row
        // and should not be processed independently.
        var auditableEntries = context.ChangeTracker
            .Entries<IAuditable>()
            .Where(e => !e.Metadata.IsOwned() &&
                        e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        // Use entry.Property(...).CurrentValue = to write directly through EF Core's
        // change-tracking layer, avoiding snapshot-refresh issues with POCO setters.
        foreach (var entry in auditableEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.CreatedByUserId)).CurrentValue = userId;
                entry.Property(nameof(IAuditable.UpdatedByUserId)).CurrentValue = userId;
            }
            else // Modified
            {
                entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.UpdatedByUserId)).CurrentValue = userId;
            }
        }

        // Materialize base-entity entries after the audit-field updates above.
        var baseEntityEntries = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => !e.Metadata.IsOwned() &&
                        e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var auditLogs = new List<AuditLog>();

        foreach (var entry in baseEntityEntries)
        {
            if (entry.Entity is AuditLog) continue;

            AuditAction? action = entry.State switch
            {
                EntityState.Added   => AuditAction.Create,
                EntityState.Modified => AuditAction.Update,
                EntityState.Deleted  => AuditAction.Delete,
                _                    => null
            };

            if (action is null) continue;

            string? oldValues = null;
            string? newValues = null;

            try
            {
                // Exclude shadow properties — they are EF Core internals and not meaningful
                // for audit logs. Accessing them can also cause issues with owned-entity state.
                if (action is AuditAction.Update or AuditAction.Delete)
                    oldValues = JsonSerializer.Serialize(
                        entry.OriginalValues.Properties
                            .Where(p => !p.IsShadowProperty())
                            .ToDictionary(p => p.Name, p => entry.OriginalValues[p]));

                if (action is AuditAction.Create or AuditAction.Update)
                    newValues = JsonSerializer.Serialize(
                        entry.CurrentValues.Properties
                            .Where(p => !p.IsShadowProperty())
                            .ToDictionary(p => p.Name, p => entry.CurrentValues[p]));
            }
            catch
            {
                // Never let audit serialization crash the actual save operation.
                oldValues = null;
                newValues = null;
            }

            auditLogs.Add(AuditLog.Create(
                userId, currentUserService.Email,
                action.Value,
                entry.Entity.GetType().Name,
                entry.Entity.Id.ToString(),
                oldValues, newValues));
        }

        if (auditLogs.Count > 0)
            context.Set<AuditLog>().AddRange(auditLogs);
    }
}
