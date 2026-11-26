using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Catalog;

/// <summary>
/// Datos de compra y margen de un producto en la Calculadora de precios (uno por producto).
/// Un frasco usa todos los campos; un decant solo guarda su margen (el costo sale de su frasco).
/// Mientras un producto no tiene entrada, la calculadora la estima a partir del costo en pesos.
/// </summary>
public class PricingCalculatorEntry
{
    public Guid ProductId { get; private set; }
    public decimal CostUsdOrigin { get; private set; }          // precio del perfume en USD en origen
    public decimal PercentageCommission { get; private set; }   // recargo % sobre el costo
    public decimal FixedCommission { get; private set; }        // comisión fija en USD (estándar o económica)
    public decimal ShippingCost { get; private set; }           // flete por unidad en USD
    public string MarginType { get; private set; } = PricingFormula.Percentage;
    public decimal MarginValue { get; private set; }            // % (ej. 35) o monto fijo en ARS (ej. 20000)
    public DateTime UpdatedAt { get; private set; }

    private PricingCalculatorEntry() { }

    public static PricingCalculatorEntry Create(
        Guid productId,
        decimal costUsdOrigin,
        decimal percentageCommission,
        decimal fixedCommission,
        decimal shippingCost,
        string marginType,
        decimal marginValue)
    {
        var entry = new PricingCalculatorEntry { ProductId = productId };
        entry.Update(costUsdOrigin, percentageCommission, fixedCommission, shippingCost, marginType, marginValue);
        return entry;
    }

    public void Update(
        decimal costUsdOrigin,
        decimal percentageCommission,
        decimal fixedCommission,
        decimal shippingCost,
        string marginType,
        decimal marginValue)
    {
        CostUsdOrigin = Math.Max(0m, costUsdOrigin);
        PercentageCommission = Math.Max(0m, percentageCommission);
        FixedCommission = Math.Max(0m, fixedCommission);
        ShippingCost = Math.Max(0m, shippingCost);
        MarginType = PricingFormula.NormalizeMarginType(marginType) ?? PricingFormula.Percentage;
        MarginValue = Math.Max(0m, marginValue);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Costo total en USD: costo + recargo % + comisión fija + flete.</summary>
    public decimal CalculateCostUsd() =>
        PricingFormula.TotalUsd(CostUsdOrigin, PercentageCommission, FixedCommission, ShippingCost);

    /// <summary>Costo total en ARS (USD × cotización USDT), redondeado a los $1.000.</summary>
    public decimal CalculateCostArs(decimal usdtRate) =>
        PricingFormula.CostArs(CalculateCostUsd(), usdtRate);

    /// <summary>Precio sugerido en ARS (costo ARS + margen), redondeado a los $1.000.</summary>
    public decimal CalculateSuggestedPriceArs(decimal usdtRate) =>
        PricingFormula.SuggestedPriceArs(CalculateCostArs(usdtRate), MarginType, MarginValue);
}
