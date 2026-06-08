using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Events.Orders;
using ChipoBackend.Domain.Exceptions;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Orders;

public class Order : AuditableEntity
{
    public string OrderNumber { get; private set; } = null!;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;

    public Address ShippingAddress { get; private set; } = null!;
    public Address? BillingAddress { get; private set; }

    public Money Subtotal { get; private set; } = null!;
    public Money DiscountAmount { get; private set; } = null!;
    public Money ShippingCost { get; private set; } = null!;
    public Money TaxAmount { get; private set; } = null!;
    public Money Total { get; private set; } = null!;

    public string? CouponCode { get; private set; }
    public string? Notes { get; private set; }

    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancelReason { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderStatusHistory> _statusHistory = [];
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Order() { }

    public static Order Create(string orderNumber, Guid customerId, Address shippingAddress, string? notes = null)
    {
        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            Notes = notes,
            Subtotal = Money.Zero(),
            DiscountAmount = Money.Zero(),
            ShippingCost = Money.Zero(),
            TaxAmount = Money.Zero(),
            Total = Money.Zero(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        order.AddStatusHistory(null, OrderStatus.Pending, "Pedido creado", null);
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, orderNumber));
        return order;
    }

    public void AddItem(Guid productId, Guid variantId, string productName, string variantDesc, string sku, int quantity, Money unitPrice, Money discount)
    {
        var item = OrderItem.Create(Id, productId, variantId, productName, variantDesc, sku, quantity, unitPrice, discount);
        _items.Add(item);
        RecalculateTotals();
    }

    public void ApplyCoupon(string couponCode, Money discountAmount)
    {
        CouponCode = couponCode;
        DiscountAmount = discountAmount;
        RecalculateTotals();
    }

    public void SetShippingCost(Money shippingCost)
    {
        ShippingCost = shippingCost;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Aggregate(Money.Zero(), (acc, i) => acc + i.Total);
        Total = Subtotal - DiscountAmount + ShippingCost + TaxAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm(Guid? changedByUserId)
    {
        EnsureStatus(OrderStatus.Pending);
        TransitionTo(OrderStatus.Confirmed, "Pedido confirmado", changedByUserId);
    }

    public void MarkAsPaid(Guid? changedByUserId)
    {
        if (Status != OrderStatus.Confirmed && Status != OrderStatus.Pending)
            throw new BusinessRuleException("InvalidOrderStatus", "Solo se puede marcar como pagado un pedido confirmado o pendiente.");
        PaidAt = DateTime.UtcNow;
        TransitionTo(OrderStatus.Paid, "Pago registrado", changedByUserId);
    }

    public void StartProcessing(Guid? changedByUserId)
    {
        EnsureStatus(OrderStatus.Paid);
        TransitionTo(OrderStatus.Processing, "En preparación", changedByUserId);
    }

    public void Ship(Guid? changedByUserId, string? note = null)
    {
        EnsureStatus(OrderStatus.Processing);
        ShippedAt = DateTime.UtcNow;
        TransitionTo(OrderStatus.Shipped, note ?? "Pedido enviado", changedByUserId);
    }

    public void Deliver(Guid? changedByUserId)
    {
        EnsureStatus(OrderStatus.Shipped);
        DeliveredAt = DateTime.UtcNow;
        TransitionTo(OrderStatus.Delivered, "Pedido entregado", changedByUserId);
    }

    public void Cancel(string reason, Guid? changedByUserId)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled || Status == OrderStatus.Refunded)
            throw new BusinessRuleException("CannotCancelOrder", $"No se puede cancelar un pedido en estado {Status}.");

        CancelledAt = DateTime.UtcNow;
        CancelReason = reason;
        TransitionTo(OrderStatus.Cancelled, $"Cancelado: {reason}", changedByUserId);
        AddDomainEvent(new OrderCancelledEvent(Id, CustomerId, OrderNumber));
    }

    private void TransitionTo(OrderStatus newStatus, string? note, Guid? userId)
    {
        var previous = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        AddStatusHistory(previous, newStatus, note, userId);
    }

    private void EnsureStatus(OrderStatus expected)
    {
        if (Status != expected)
            throw new BusinessRuleException("InvalidOrderStatus", $"Se esperaba estado {expected}, pero el pedido está en {Status}.");
    }

    private void AddStatusHistory(OrderStatus? from, OrderStatus to, string? note, Guid? userId)
    {
        _statusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = from,
            ToStatus = to,
            Note = note,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow
        });
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Paid,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}
