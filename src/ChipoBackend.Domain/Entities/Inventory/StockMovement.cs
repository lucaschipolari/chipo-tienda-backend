using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Inventory;

public class StockMovement : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public MovementType MovementType { get; private set; }
    public int Quantity { get; private set; }
    public int StockBefore { get; private set; }
    public int StockAfter { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
    public string? Reason { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(
        Guid productId, Guid variantId, MovementType type,
        int quantity, int stockBefore, int stockAfter,
        Guid? referenceId = null, string? referenceType = null,
        string? reason = null, Guid? createdByUserId = null)
    {
        return new StockMovement
        {
            ProductId = productId,
            VariantId = variantId,
            MovementType = type,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            ReferenceId = referenceId,
            ReferenceType = referenceType,
            Reason = reason,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

public enum MovementType
{
    Initial,
    PurchaseReceipt,
    SaleOut,
    OrderReservation,
    OrderRelease,
    ManualAdjustment,
    Return,
    Damage
}
