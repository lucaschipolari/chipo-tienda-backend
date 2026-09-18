using System.Text.RegularExpressions;
using ChipoBackend.Domain.Entities.Catalog;

namespace ChipoBackend.Application.Features.Profitability;

/// <summary>
/// Resuelve el "último costo" y el "costo anterior" de una variante según las reglas del negocio:
///  - Decant  → costo calculado (ml × costo/ml del frasco + frasquito). Sin costo anterior por ahora.
///  - Normal  → última y penúltima compra recibida (historial); si no hay historial, cae al costo manual.
/// </summary>
public static class ProfitabilityCostResolver
{
    /// <summary>ml de una variante decant desde sus atributos (ej "5ml" → 5). 0 si no hay.</summary>
    public static int ParseMl(IReadOnlyDictionary<string, string>? attributes)
    {
        if (attributes is null) return 0;
        foreach (var v in attributes.Values)
        {
            var m = Regex.Match(v ?? "", @"(\d+)\s*ml", RegexOptions.IgnoreCase);
            if (m.Success) return int.Parse(m.Groups[1].Value);
        }
        return 0;
    }

    /// <summary>Costo unitario de una variante decant (líquido + frasquito). Null si falta info.</summary>
    public static decimal? DecantUnitCost(Product product, ProductVariant variant, IReadOnlyDictionary<int, decimal> vialCosts)
    {
        var costPerMl = product.CostPerMl;
        if (costPerMl is null) return null;
        var ml = ParseMl(variant.Attributes);
        if (ml <= 0) return null;
        var vial = vialCosts.TryGetValue(ml, out var vc) ? vc : 0m;
        return ml * costPerMl.Value + vial;
    }

    /// <summary>
    /// Devuelve (últimoCosto, costoAnterior) para una variante.
    /// <paramref name="variantHistoryDesc"/> debe venir ordenado del más reciente al más antiguo.
    /// </summary>
    public static (decimal? Last, decimal? Previous) Resolve(
        Product product,
        ProductVariant variant,
        IReadOnlyList<ProductCostHistory> variantHistoryDesc,
        IReadOnlyDictionary<int, decimal> vialCosts)
    {
        if (product.IsDecant)
            return (DecantUnitCost(product, variant, vialCosts), null);

        if (variantHistoryDesc.Count > 0)
        {
            var last = variantHistoryDesc[0].UnitCost.Amount;
            decimal? prev = variantHistoryDesc.Count > 1 ? variantHistoryDesc[1].UnitCost.Amount : null;
            return (last, prev);
        }

        // Sin historial de compras → costo manual de la variante como respaldo.
        return (variant.Cost?.Amount, null);
    }

    /// <summary>Precio de venta efectivo de la variante (su precio, o el precio base del producto).</summary>
    public static decimal? SalePrice(Product product, ProductVariant variant)
    {
        var p = variant.Price?.Amount;
        if (p is > 0) return p;
        var basePrice = product.BasePrice?.Amount;
        return basePrice is > 0 ? basePrice : null;
    }

    public static string? VariantLabel(ProductVariant variant) =>
        variant.Attributes is { Count: > 0 } ? string.Join(" · ", variant.Attributes.Values) : null;
}
