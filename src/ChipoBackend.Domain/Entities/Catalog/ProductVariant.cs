using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Catalog;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string? Barcode { get; private set; }
    public Dictionary<string, string> Attributes { get; private set; } = [];
    public Money? Price { get; private set; }
    public Money? CompareAtPrice { get; private set; }   // precio tachado (descuento) por variante
    public Money? Cost { get; private set; }             // costo de reposición (para calcular ganancia)
    public int StockQuantity { get; private set; }
    public int MinStockThreshold { get; private set; } = 5;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ProductVariant() { }

    public static ProductVariant Create(Guid productId, string sku, Dictionary<string, string> attributes, int initialStock, Money? price, int minStockThreshold, Money? compareAtPrice = null, Money? cost = null)
    {
        return new ProductVariant
        {
            ProductId = productId,
            Sku = sku,
            Attributes = attributes,
            StockQuantity = initialStock,
            Price = price,
            CompareAtPrice = compareAtPrice,
            Cost = cost,
            MinStockThreshold = minStockThreshold,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateAttributes(Dictionary<string, string> attributes)
    {
        Attributes = attributes ?? [];
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Money? price)
    {
        Price = price;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCompareAtPrice(Money? compareAtPrice)
    {
        CompareAtPrice = compareAtPrice;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCost(Money? cost)
    {
        Cost = cost;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int newQuantity)
    {
        StockQuantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementStock(int quantity)
    {
        StockQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementStock(int quantity)
    {
        StockQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsBelowMinStock => StockQuantity <= MinStockThreshold;

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
}
