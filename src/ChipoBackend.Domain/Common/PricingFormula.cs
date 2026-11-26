namespace ChipoBackend.Domain.Common;

/// <summary>
/// Fórmula de la Calculadora de precios:
/// total USD = round2(costo + costo × % + comisión fija + flete); costo ARS = PriceRounding(total USD × USDT);
/// precio sugerido = PriceRounding(costo ARS + margen fijo) o PriceRounding(costo ARS × (1 + margen %)).
/// El margen es un recargo sobre el costo (markup), no el margen sobre el precio que usa Rentabilidad.
/// </summary>
public static class PricingFormula
{
    public const string Percentage = "Percentage";
    public const string FixedAmount = "FixedAmount";

    /// <summary>Recargo porcentual en USD sobre el costo (sin redondear).</summary>
    public static decimal CommissionUsd(decimal costUsd, decimal commissionPercentage) =>
        costUsd * (commissionPercentage / 100m);

    /// <summary>Costo total en USD redondeado a 2 decimales.</summary>
    public static decimal TotalUsd(decimal costUsd, decimal commissionPercentage, decimal fixedCommission, decimal shippingCost) =>
        Math.Round(costUsd + CommissionUsd(costUsd, commissionPercentage) + fixedCommission + shippingCost, 2, MidpointRounding.AwayFromZero);

    /// <summary>Costo en ARS (total USD × cotización USDT) redondeado a los $1.000; 0 si la cotización no es positiva.</summary>
    public static decimal CostArs(decimal totalUsd, decimal usdtRate) =>
        usdtRate <= 0 ? 0 : PriceRounding.Round(totalUsd * usdtRate);

    /// <summary>Precio sugerido en ARS: margen fijo (FixedAmount) o porcentual (cualquier otro valor), redondeado a los $1.000.</summary>
    public static decimal SuggestedPriceArs(decimal costArs, string marginType, decimal marginValue) =>
        PriceRounding.Round(ApplyMargin(costArs, marginType, marginValue));

    /// <summary>Costo + margen, sin redondear (los decants redondean al peso, no a los $1.000).</summary>
    public static decimal ApplyMargin(decimal cost, string marginType, decimal marginValue) =>
        marginType.Equals(FixedAmount, StringComparison.OrdinalIgnoreCase)
            ? cost + marginValue
            : cost * (1m + marginValue / 100m);

    /// <summary>
    /// Costo en USD de origen que, con estos gastos, da un costo total de <paramref name="totalUsd"/>
    /// (la fórmula al revés). Se usa para estimar el punto de partida de un producto que todavía no
    /// se cargó en la calculadora, a partir de su costo en pesos.
    /// </summary>
    public static decimal EstimateOriginUsd(decimal totalUsd, decimal commissionPercentage, decimal fixedCommission, decimal shippingCost)
    {
        var raw = (totalUsd - (fixedCommission + shippingCost)) / (1m + commissionPercentage / 100m);
        return Math.Max(0m, Math.Round(raw, 2, MidpointRounding.AwayFromZero));
    }

    /// <summary>Devuelve el tipo de margen canónico ("Percentage" o "FixedAmount") o null si no es válido.</summary>
    public static string? NormalizeMarginType(string? marginType)
    {
        if (string.Equals(marginType, Percentage, StringComparison.OrdinalIgnoreCase)) return Percentage;
        if (string.Equals(marginType, FixedAmount, StringComparison.OrdinalIgnoreCase)) return FixedAmount;
        return null;
    }
}
