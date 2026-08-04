using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Combos;

public class ComboItem : BaseEntity
{
    public Guid ComboId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }

    private ComboItem() { }

    public static ComboItem Create(Guid comboId, Guid productId, Guid variantId, int quantity) => new()
    {
        ComboId = comboId,
        ProductId = productId,
        VariantId = variantId,
        Quantity = quantity < 1 ? 1 : quantity,
    };
}
