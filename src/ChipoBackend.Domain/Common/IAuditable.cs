namespace ChipoBackend.Domain.Common;

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
    Guid? CreatedByUserId { get; }
    Guid? UpdatedByUserId { get; }
}
