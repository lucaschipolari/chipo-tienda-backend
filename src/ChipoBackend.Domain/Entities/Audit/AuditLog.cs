using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Audit;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = null!;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime OccurredAt { get; set; }

    public static AuditLog Create(Guid? userId, string? userEmail, AuditAction action, string entityName, string? entityId, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null)
    {
        return new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            OccurredAt = DateTime.UtcNow
        };
    }
}

public enum AuditAction { Create, Update, Delete, Login, Logout, Export, StatusChange, PasswordChange }
