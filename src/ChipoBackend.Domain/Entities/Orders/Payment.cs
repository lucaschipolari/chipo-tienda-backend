using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Orders;

public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; }
    public string Method { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string? GatewayRef { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Payment() { }

    public static Payment Create(Guid orderId, string method, Money amount)
    {
        return new Payment { OrderId = orderId, Method = method, Amount = amount, CreatedAt = DateTime.UtcNow };
    }

    public void MarkAsCompleted(string? gatewayRef = null)
    {
        Status = PaymentStatus.Completed;
        GatewayRef = gatewayRef;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed() => Status = PaymentStatus.Failed;
    public void MarkAsRefunded() => Status = PaymentStatus.Refunded;
}

public enum PaymentStatus { Pending, Completed, Failed, Refunded }
