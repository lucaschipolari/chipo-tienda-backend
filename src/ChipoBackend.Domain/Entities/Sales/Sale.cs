using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Sales;

public class Sale : AuditableEntity
{
    public string SaleNumber { get; private set; } = null!;
    public Guid? CustomerId { get; private set; }
    public Guid SoldByUserId { get; private set; }
    public SaleChannel Channel { get; private set; }
    public Money Subtotal { get; private set; } = null!;
    public Money DiscountAmount { get; private set; } = null!;
    public Money Total { get; private set; } = null!;
    public string PaymentMethod { get; private set; } = null!;
    public string? Notes { get; private set; }

    private readonly List<SaleItem> _items = [];
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private Sale() { }

    public static Sale Create(string saleNumber, Guid soldByUserId, string paymentMethod, SaleChannel channel, string currency = "ARS", Guid? customerId = null, string? notes = null)
    {
        return new Sale
        {
            SaleNumber = saleNumber,
            SoldByUserId = soldByUserId,
            CustomerId = customerId,
            Channel = channel,
            PaymentMethod = paymentMethod,
            Notes = notes,
            Subtotal = Money.Zero(currency),
            DiscountAmount = Money.Zero(currency),
            Total = Money.Zero(currency),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid productId, Guid variantId, string productName, string sku, int quantity, Money unitPrice, Money discount, Money? unitCost = null)
    {
        _items.Add(SaleItem.Create(Id, productId, variantId, productName, sku, quantity, unitPrice, discount, unitCost));
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        var currency = Subtotal.Currency;
        Subtotal = _items.Aggregate(Money.Zero(currency), (acc, i) => acc + i.Total);
        Total = Subtotal - DiscountAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Costo total de la venta (suma de costos de cada línea).</summary>
    public Money TotalCost =>
        _items.Aggregate(Money.Zero(Subtotal.Currency), (acc, i) => acc + i.TotalCost);

    /// <summary>Ganancia real de la venta (total cobrado − costo total).</summary>
    public Money Profit => Total - TotalCost;
}

public enum SaleChannel { InStore, Phone, WhatsApp, Other }
