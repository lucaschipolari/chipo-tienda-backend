using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Events.Purchases;
using ChipoBackend.Domain.Exceptions;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Purchasing;

public class PurchaseOrder : AuditableEntity
{
    public string PurchaseNumber { get; private set; } = null!;
    public Guid SupplierId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; } = PurchaseOrderStatus.Draft;
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public Money Subtotal { get; private set; } = null!;
    public Money TaxAmount { get; private set; } = null!;
    public Money Total { get; private set; } = null!;
    public string? Notes { get; private set; }
    public string Currency { get; private set; } = "ARS";

    private readonly List<PurchaseOrderItem> _items = [];
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    private PurchaseOrder() { }

    public static PurchaseOrder Create(string purchaseNumber, Guid supplierId, Guid? createdByUser, DateTime? expectedDate = null, string currency = "ARS")
    {
        return new PurchaseOrder
        {
            PurchaseNumber = purchaseNumber,
            SupplierId = supplierId,
            CreatedByUserId = createdByUser,
            ExpectedDeliveryDate = expectedDate,
            Currency = currency,
            Subtotal = Money.Zero(currency),
            TaxAmount = Money.Zero(currency),
            Total = Money.Zero(currency),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, Guid variantId, int quantity, Money unitCost)
    {
        EnsureStatus(PurchaseOrderStatus.Draft);
        var item = PurchaseOrderItem.Create(Id, productId, variantId, quantity, unitCost);
        _items.Add(item);
        RecalculateTotals();
    }

    public void Send()
    {
        EnsureStatus(PurchaseOrderStatus.Draft);
        if (!_items.Any())
            throw new BusinessRuleException("EmptyPurchaseOrder", "La orden de compra no tiene ítems.");
        Status = PurchaseOrderStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        EnsureStatus(PurchaseOrderStatus.Sent);
        Status = PurchaseOrderStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReceiveItems(Dictionary<Guid, int> itemReceipts)
    {
        if (Status != PurchaseOrderStatus.Sent && Status != PurchaseOrderStatus.Approved && Status != PurchaseOrderStatus.PartiallyReceived)
            throw new BusinessRuleException("InvalidPurchaseStatus", "Solo se puede recibir una orden enviada, aprobada o parcialmente recibida.");

        foreach (var (itemId, quantityReceived) in itemReceipts)
        {
            var item = _items.FirstOrDefault(i => i.Id == itemId)
                ?? throw new DomainException($"Ítem {itemId} no encontrado en la orden.");
            item.Receive(quantityReceived);
        }

        Status = _items.All(i => i.IsFullyReceived) ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PurchaseReceivedEvent(Id, SupplierId, PurchaseNumber, itemReceipts));
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Received)
            throw new BusinessRuleException("CannotCancelReceived", "No se puede cancelar una orden ya recibida.");
        Status = PurchaseOrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Aggregate(Money.Zero(Currency), (acc, i) => acc + i.Total);
        Total = Subtotal + TaxAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureStatus(PurchaseOrderStatus expected)
    {
        if (Status != expected)
            throw new BusinessRuleException("InvalidPurchaseStatus", $"Se esperaba estado {expected}, pero la orden está en {Status}.");
    }
}

public enum PurchaseOrderStatus { Draft, Sent, Approved, PartiallyReceived, Received, Cancelled }
