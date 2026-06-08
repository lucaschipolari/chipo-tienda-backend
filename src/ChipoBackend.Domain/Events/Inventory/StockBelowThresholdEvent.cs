using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Events.Inventory;

public record StockBelowThresholdEvent(Guid ProductId, Guid VariantId, string Sku, int CurrentStock, int MinThreshold) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
