namespace ChipoBackend.Application.Features.Profitability;

/// <summary>Estado de rentabilidad de un producto respecto de su margen objetivo.</summary>
public static class ProfitabilityStatus
{
    public const string Optimo = "Optimo";     // 🟢 margen >= objetivo
    public const string Bajo = "Bajo";         // 🟡 por debajo del objetivo pero dentro de la banda
    public const string Critico = "Critico";   // 🔴 muy por debajo del objetivo
    public const string SinDatos = "SinDatos"; // falta precio o costo válido
}

/// <summary>Resultado del análisis de rentabilidad de una variante/producto.</summary>
public record ProfitabilityResult(
    decimal? SalePrice,
    decimal? LastCost,
    decimal? PreviousCost,
    decimal? CostChangeAbs,
    decimal? CostChangePct,
    decimal? Profit,
    decimal? MarginPct,
    decimal TargetMarginPct,
    bool TargetIsCustom,
    decimal? SuggestedPrice,
    decimal? PriceDifference,
    string Status
);

/// <summary>
/// Núcleo de cálculo de rentabilidad. Funciones puras, sin dependencias de infraestructura,
/// para que la lógica de negocio viva en el backend y sea testeable.
/// Métrica principal: MARGEN sobre el precio de venta (no markup).
/// </summary>
public static class ProfitabilityCalculator
{
    /// <summary>Margen % = ((precio - costo) / precio) × 100. Null si el precio no es válido.</summary>
    public static decimal? Margin(decimal? salePrice, decimal? cost)
    {
        if (salePrice is null or <= 0 || cost is null or < 0) return null;
        return (salePrice.Value - cost.Value) / salePrice.Value * 100m;
    }

    /// <summary>Precio necesario para alcanzar un margen objetivo: costo / (1 - margen).</summary>
    public static decimal? SuggestedPrice(decimal? cost, decimal targetMarginPct, int roundingStep, string roundingMode)
    {
        if (cost is null or <= 0) return null;
        if (targetMarginPct is < 0 or >= 100) return null;
        var raw = cost.Value / (1m - targetMarginPct / 100m);
        return Round(raw, roundingStep, roundingMode);
    }

    /// <summary>Redondea según el paso y modo configurados. step &lt;= 0 → sin redondeo.</summary>
    public static decimal Round(decimal value, int step, string mode)
    {
        if (step <= 0) return decimal.Round(value, 2);
        var q = value / step;
        var rounded = mode switch
        {
            "up" => Math.Ceiling(q),
            "down" => Math.Floor(q),
            _ => Math.Round(q, MidpointRounding.AwayFromZero)
        };
        return rounded * step;
    }

    /// <summary>Variación de costo entre el anterior y el último (abs y %).</summary>
    public static (decimal? Abs, decimal? Pct) CostChange(decimal? previous, decimal? last)
    {
        if (previous is null || last is null) return (null, null);
        var abs = last.Value - previous.Value;
        var pct = previous.Value != 0 ? abs / previous.Value * 100m : (decimal?)null;
        return (abs, pct);
    }

    /// <summary>Determina el estado según margen actual, objetivo y banda crítica.</summary>
    public static string Status(decimal? marginPct, decimal targetMarginPct, decimal criticalBandPct)
    {
        if (marginPct is null) return ProfitabilityStatus.SinDatos;
        if (marginPct.Value >= targetMarginPct) return ProfitabilityStatus.Optimo;
        if (marginPct.Value >= targetMarginPct - criticalBandPct) return ProfitabilityStatus.Bajo;
        return ProfitabilityStatus.Critico;
    }

    /// <summary>Análisis completo de una variante/producto.</summary>
    public static ProfitabilityResult Analyze(
        decimal? salePrice, decimal? lastCost, decimal? previousCost,
        decimal? productTargetMargin, ProfitabilitySettingsDto settings)
    {
        var target = productTargetMargin ?? settings.TargetMarginPct;
        var isCustom = productTargetMargin.HasValue;

        var margin = Margin(salePrice, lastCost);
        var profit = (salePrice.HasValue && lastCost.HasValue) ? salePrice.Value - lastCost.Value : (decimal?)null;
        var suggested = SuggestedPrice(lastCost, target, settings.RoundingStep, settings.RoundingMode);
        var diff = (suggested.HasValue && salePrice.HasValue) ? suggested.Value - salePrice.Value : (decimal?)null;
        var (costAbs, costPct) = CostChange(previousCost, lastCost);
        var status = Status(margin, target, settings.CriticalBandPct);

        return new ProfitabilityResult(
            SalePrice: salePrice,
            LastCost: lastCost,
            PreviousCost: previousCost,
            CostChangeAbs: costAbs,
            CostChangePct: costPct,
            Profit: profit,
            MarginPct: margin,
            TargetMarginPct: target,
            TargetIsCustom: isCustom,
            SuggestedPrice: suggested,
            PriceDifference: diff,
            Status: status
        );
    }
}
