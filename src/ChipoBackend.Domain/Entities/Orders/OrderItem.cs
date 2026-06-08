using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Orders;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string ProductName { get; private set; } = null!;
    public string VariantDescription { get; private set; } = null!;
    public string Sku { get; private set; } = null!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money Discount { get; private set; } = null!;
    public Money Total { get; private set; } = null!;

    private OrderItem() { }

    public static OrderItem Create(Guid orderId, Guid productId, Guid variantId, string productName, string variantDesc, string sku, int quantity, Money unitPrice, Money discount)
    {
        var total = (unitPrice * quantity) - discount;
        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            VariantId = variantId,
            ProductName = productName,
            VariantDescription = variantDesc,
            Sku = sku,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = discount,
            Total = total
        };
    }
}
