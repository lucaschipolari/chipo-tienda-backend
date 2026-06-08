using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Inventory;

public class LostSale : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public int QuantityRequested { get; private set; }
    public LostSaleSource Source { get; private set; }
    public string? Notes { get; private set; }
    public Guid? ReportedByUserId { get; private set; }
    public DateTime OccurredAt { get; private set; }

    private LostSale() { }

    public static LostSale Create(Guid productId, Guid? variantId, int quantityRequested, LostSaleSource source, string? notes, Guid? reportedByUserId)
    {
        return new LostSale
        {
            ProductId = productId,
            VariantId = variantId,
            QuantityRequested = quantityRequested,
            Source = source,
            Notes = notes,
            ReportedByUserId = reportedByUserId,
            OccurredAt = DateTime.UtcNow
        };
    }
}

public enum LostSaleSource { Store, Online, Phone, Other }
