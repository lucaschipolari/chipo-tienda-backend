using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Sales;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money Discount { get; private set; } = null!;
    public Money Total { get; private set; } = null!;

    private SaleItem() { }

    public static SaleItem Create(Guid saleId, Guid productId, Guid variantId, string productName, string sku, int quantity, Money unitPrice, Money discount)
    {
        return new SaleItem
        {
            SaleId = saleId,
            ProductId = productId,
            VariantId = variantId,
            ProductName = productName,
            Sku = sku,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = discount,
            Total = (unitPrice * quantity) - discount
        };
    }
}
