using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Catalog;

/// <summary>Origen de un registro de costo (por qué se guardó).</summary>
public enum CostSource
{
    Initial,          // carga/backfill inicial
    PurchaseReceipt,  // se recibió una orden de compra
    ManualEdit        // el admin editó el costo de la variante a mano
}

/// <summary>
/// Registro histórico (append-only) del costo de compra de una variante.
/// Nunca se pisa ni se borra: cada compra recibida o edición de costo agrega una fila.
/// </summary>
public class ProductCostHistory : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public Money UnitCost { get; private set; } = null!;
    public CostSource Source { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }   // si vino de una recepción
    public DateTime RecordedAt { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    private ProductCostHistory() { }

    public static ProductCostHistory Create(
        Guid productId, Guid variantId, Money unitCost, CostSource source,
        Guid? purchaseOrderId = null, Guid? createdByUserId = null, DateTime? recordedAt = null)
    {
        return new ProductCostHistory
        {
            ProductId = productId,
            VariantId = variantId,
            UnitCost = unitCost,
            Source = source,
            PurchaseOrderId = purchaseOrderId,
            CreatedByUserId = createdByUserId,
            RecordedAt = recordedAt ?? DateTime.UtcNow
        };
    }
}
