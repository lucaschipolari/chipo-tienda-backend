using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Events.Purchases;

public record PurchaseReceivedEvent(Guid PurchaseOrderId, Guid SupplierId, string PurchaseNumber, Dictionary<Guid, int> ReceivedItems) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
