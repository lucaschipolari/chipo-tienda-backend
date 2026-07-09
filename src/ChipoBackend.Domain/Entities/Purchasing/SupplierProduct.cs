using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Purchasing;

public class SupplierProduct : BaseEntity
{
    public Guid SupplierId { get; private set; }
    public Guid ProductId { get; private set; }
    public string? SupplierProductCode { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public string Currency { get; private set; } = "ARS";
    public int LeadTimeDays { get; private set; }
    public bool IsPreferredSupplier { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private SupplierProduct() { }

    public static SupplierProduct Create(
        Guid supplierId,
        Guid productId,
        decimal purchasePrice,
        string? supplierProductCode = null,
        string currency = "ARS",
        int leadTimeDays = 0,
        bool isPreferredSupplier = false)
    {
        return new SupplierProduct
        {
            SupplierId = supplierId,
            ProductId = productId,
            PurchasePrice = purchasePrice,
            SupplierProductCode = supplierProductCode,
            Currency = currency,
            LeadTimeDays = leadTimeDays,
            IsPreferredSupplier = isPreferredSupplier,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        decimal purchasePrice,
        string? supplierProductCode,
        string currency,
        int leadTimeDays,
        bool isPreferredSupplier)
    {
        PurchasePrice = purchasePrice;
        SupplierProductCode = supplierProductCode;
        Currency = currency;
        LeadTimeDays = leadTimeDays;
        IsPreferredSupplier = isPreferredSupplier;
        UpdatedAt = DateTime.UtcNow;
    }
}
