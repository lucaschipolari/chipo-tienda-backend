using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Events.Orders;

public record OrderCancelledEvent(Guid OrderId, Guid? CustomerId, string OrderNumber) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
