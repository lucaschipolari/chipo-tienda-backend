using ChipoBackend.Application.Features.Profitability;
using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Application.Features.PricingCalculator;

/// <summary>
/// Cálculos de la Calculadora de precios, en un solo lugar para la consulta y los comandos.
///
/// Frasco: costo ARS = (USD origen + recargo % + comisión fija + flete) × USDT, redondeado a los $1.000;
///         sugerido = costo + margen del grupo (o el del producto), redondeado a los $1.000.
///         Sin entrada propia, el USD de origen se estima desde el costo en pesos de la variante principal.
/// Decant: por tamaño, costo = ⌈ml × costo por ml del frasco⌉ + frasquito de ese tamaño + extras;
///         sugerido = costo + margen, redondeado al peso.
/// </summary>
public static class PricingCalculatorBuilder
{
    /// <summary>Un tamaño de decant ya calculado.</summary>
    public record DecantPrice(ProductVariant Variant, int Ml, decimal LiquidCost, decimal Packaging, decimal Cost, decimal Suggested);

    /// <summary>Variante principal de un frasco: la primera activa por orden.</summary>
    public static ProductVariant? MainVariant(Product product) =>
        product.Variants.Where(v => v.IsActive).OrderBy(v => v.DisplayOrder).FirstOrDefault()
        ?? product.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault();

    public static string GroupFor(Product product) => PricingSettings.GroupFor(product.IsDecant, product.Category?.Slug);

    public static decimal CurrentPrice(Product product, ProductVariant? variant) =>
        variant?.Price?.Amount ?? product.BasePrice.Amount;

    /// <summary>Entrada estimada desde el costo en pesos de la variante principal; null si no hay costo.</summary>
    public static PricingCalculatorEntry? EstimateEntry(Product product, PricingSettingsDto settings)
    {
        var costArs = MainVariant(product)?.Cost?.Amount;
        if (costArs is not > 0 || settings.UsdtRate <= 0) return null;

        var origin = PricingFormula.EstimateOriginUsd(
            costArs.Value / settings.UsdtRate,
            settings.CommissionPercentage,
            settings.CommissionFixedStandard,
            settings.ShippingCost);
        var (type, value) = PricingSettings.MarginFor(GroupFor(product), settings);

        return PricingCalculatorEntry.Create(
            product.Id, origin, settings.CommissionPercentage, settings.CommissionFixedStandard,
            settings.ShippingCost, type, value);
    }

    /// <summary>Tamaños activos de un decant con sus ml, del más chico al más grande.</summary>
    public static List<(ProductVariant Variant, int Ml)> DecantSizes(Product product) =>
        product.Variants
            .Where(v => v.IsActive)
            .Select(v => (Variant: v, Ml: ProfitabilityCostResolver.ParseMl(v.Attributes)))
            .Where(x => x.Ml > 0)
            .OrderBy(x => x.Ml)
            .ToList();

    /// <summary>Precios de cada tamaño activo de un decant (ordenados por ml). Sin costo de frasco, el sugerido es 0.</summary>
    public static List<DecantPrice> PriceDecant(Product product, string marginType, decimal marginValue, PricingSettingsDto settings)
    {
        var costPerMl = product.CostPerMl ?? 0m;
        return DecantSizes(product)
            .Select(x =>
            {
                var liquid = costPerMl > 0 ? Math.Ceiling(costPerMl * x.Ml) : 0m;
                var packaging = PricingSettings.PackagingFor(x.Ml, settings);
                var cost = liquid + packaging;
                var suggested = liquid > 0
                    ? Math.Round(PricingFormula.ApplyMargin(cost, marginType, marginValue), 0, MidpointRounding.AwayFromZero)
                    : 0m;
                return new DecantPrice(x.Variant, x.Ml, liquid, packaging, cost, suggested);
            })
            .ToList();
    }

    public static ProductPricingItemDto Build(Product product, PricingCalculatorEntry? entry, PricingSettingsDto settings)
    {
        var group = GroupFor(product);
        var currency = product.BasePrice.Currency;
        var image = product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.Url;
        var categoryName = product.Category?.Name ?? "Sin categoría";
        var updatedAt = entry?.UpdatedAt ?? product.UpdatedAt;

        if (product.IsDecant)
        {
            var (defType, defValue) = PricingSettings.MarginFor(group, settings);
            var marginType = entry?.MarginType ?? defType;
            var marginValue = entry?.MarginValue ?? defValue;

            var sizes = PriceDecant(product, marginType, marginValue, settings)
                .Select(d =>
                {
                    var current = CurrentPrice(product, d.Variant);
                    return new DecantVariantPricingDto(
                        d.Variant.Id, d.Variant.Sku, d.Ml, d.LiquidCost, d.Packaging, d.Cost, d.Suggested, current,
                        HasPriceDifference: d.Suggested > 0 && d.Suggested - current >= 1m,
                        ProfitArs: d.LiquidCost > 0 ? current - d.Cost : 0m);
                })
                .ToList();
            var first = sizes.FirstOrDefault();

            return new ProductPricingItemDto(
                product.Id, product.Name, product.Sku, image, product.CategoryId, categoryName, group,
                IsDecant: true, HasEntry: entry is not null, currency,
                CostUsdOrigin: 0m, PercentageCommission: 0m, FixedCommission: 0m, ShippingCost: 0m, TotalCostUsd: 0m,
                TotalCostArs: product.BottleCost ?? 0m,
                marginType, marginValue,
                SuggestedPriceArs: first?.SuggestedPriceArs ?? 0m,
                CurrentPriceArs: first?.CurrentPriceArs ?? product.BasePrice.Amount,
                HasPriceDifference: sizes.Any(s => s.HasPriceDifference),
                ProfitArs: first?.ProfitArs ?? 0m,
                updatedAt,
                BottleCostArs: product.BottleCost, BottleMl: product.BottleMl, CostPerMl: product.CostPerMl,
                DecantVariants: sizes);
        }

        var effective = entry ?? EstimateEntry(product, settings);
        var currentPrice = CurrentPrice(product, MainVariant(product));

        if (effective is null)
        {
            // Sin costo cargado: se muestra para que se complete el precio en USD, sin inventar un sugerido.
            var (type, value) = PricingSettings.MarginFor(group, settings);
            return new ProductPricingItemDto(
                product.Id, product.Name, product.Sku, image, product.CategoryId, categoryName, group,
                IsDecant: false, HasEntry: false, currency,
                CostUsdOrigin: 0m, settings.CommissionPercentage, settings.CommissionFixedStandard, settings.ShippingCost,
                TotalCostUsd: 0m, TotalCostArs: 0m, type, value,
                SuggestedPriceArs: 0m, currentPrice, HasPriceDifference: false, ProfitArs: 0m, updatedAt,
                null, null, null, []);
        }

        var costArs = effective.CalculateCostArs(settings.UsdtRate);
        var suggested = effective.CalculateSuggestedPriceArs(settings.UsdtRate);

        return new ProductPricingItemDto(
            product.Id, product.Name, product.Sku, image, product.CategoryId, categoryName, group,
            IsDecant: false, HasEntry: entry is not null, currency,
            effective.CostUsdOrigin, effective.PercentageCommission, effective.FixedCommission, effective.ShippingCost,
            TotalCostUsd: effective.CalculateCostUsd(), TotalCostArs: costArs,
            effective.MarginType, effective.MarginValue,
            SuggestedPriceArs: suggested, CurrentPriceArs: currentPrice,
            HasPriceDifference: suggested > 0 && suggested - currentPrice >= 1m,
            ProfitArs: costArs > 0 ? currentPrice - costArs : 0m,
            updatedAt,
            null, null, null, []);
    }

    /// <summary>
    /// Guarda en la variante el costo que calculó la calculadora y deja constancia en el historial de costos
    /// (solo si cambió y es positivo, igual que la edición manual de costos).
    /// </summary>
    public static void SyncVariantCost(Product product, ProductVariant variant, decimal cost, IUnitOfWork unitOfWork, Guid? userId)
    {
        if (cost <= 0 || variant.Cost?.Amount == cost) return;

        var money = Money.Of(cost, product.BasePrice.Currency);
        variant.UpdateCost(money);
        unitOfWork.Add(ProductCostHistory.Create(product.Id, variant.Id, money, CostSource.PricingCalculator, createdByUserId: userId));
    }

    /// <summary>Cambia el precio base del producto sin tocar el resto de sus datos.</summary>
    public static void SetBasePrice(Product product, decimal price) =>
        product.Update(product.Name, product.Slug, product.CategoryId, product.Description,
            Money.Of(price, product.BasePrice.Currency), product.CompareAtPrice, product.IsFeatured, product.Tags);
}
