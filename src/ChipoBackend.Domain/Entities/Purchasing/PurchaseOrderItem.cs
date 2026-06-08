using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Exceptions;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Purchasing;

public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public int QuantityOrdered { get; private set; }
    public int QuantityReceived { get; private set; }
    public Money UnitCost { get; private set; } = null!;
    public Money Total { get; private set; } = null!;
    public string? Notes { get; private set; }

    public bool IsFullyReceived => QuantityReceived >= QuantityOrdered;
    public int PendingQuantity => QuantityOrdered - QuantityReceived;

    private PurchaseOrderItem() { }

    public static PurchaseOrderItem Create(Guid purchaseOrderId, Guid productId, Guid variantId, int quantity, Money unitCost)
    {
        return new PurchaseOrderItem
        {
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            VariantId = variantId,
            QuantityOrdered = quantity,
            UnitCost = unitCost,
            Total = unitCost * quantity
        };
    }

    public void Receive(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad recibida debe ser mayor a cero.");
        if (QuantityReceived + quantity > QuantityOrdered)
            throw new BusinessRuleException("ExcessReceiving", $"La cantidad recibida ({QuantityReceived + quantity}) supera la ordenada ({QuantityOrdered}).");
        QuantityReceived += quantity;
    }
}
