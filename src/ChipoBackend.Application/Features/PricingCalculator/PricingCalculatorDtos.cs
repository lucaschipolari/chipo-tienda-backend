using ChipoBackend.Application.Features.Settings;

namespace ChipoBackend.Application.Features.PricingCalculator;

/// <summary>
/// Margen por defecto de un grupo de categorías. Grupos: "arabes", "disenador", "nicho", "decants"
/// (los testers caen en el grupo de su línea: "Diseñador Testers" → diseñador).
/// </summary>
public record GroupMarginDto(string Group, string Label, string MarginType, decimal MarginValue);

/// <summary>Extra de packaging con nombre y costo en ARS (ej. "Bolsita" $300), que se suma a todos los decants.</summary>
public record PackagingExtraDto(string Name, decimal Cost);

public record PricingSettingsDto(
    decimal UsdtRate,
    decimal CommissionPercentage,
    decimal CommissionFixedStandard,
    decimal CommissionFixedCheap,
    decimal ShippingCost,
    List<GroupMarginDto> Margins,
    // Costo del frasquito por tamaño: es el mismo ajuste de Inventario (no se edita desde la calculadora).
    List<VialCostDto> VialCosts,
    List<PackagingExtraDto> DecantPackagingExtras
);

/// <summary>Precio de un tamaño de decant: costo exacto (ml × costo/ml) + frasquito + extras.</summary>
public record DecantVariantPricingDto(
    Guid VariantId,
    string Sku,
    int Ml,
    decimal LiquidCostArs,      // ml × costo por ml, redondeado hacia arriba al peso
    decimal PackagingArs,       // frasquito de ese tamaño + extras
    decimal CostArs,            // líquido + packaging
    decimal SuggestedPriceArs,  // costo + margen, redondeado al peso
    decimal CurrentPriceArs,
    bool HasPriceDifference,    // el precio actual está por debajo del sugerido
    decimal ProfitArs           // precio actual − costo
);

public record ProductPricingItemDto(
    Guid ProductId,
    string ProductName,
    string Sku,
    string? ImageUrl,
    Guid CategoryId,
    string CategoryName,
    string Group,
    bool IsDecant,
    // false = todavía no se cargó en la calculadora: los valores en USD son una estimación desde el costo en pesos.
    bool HasEntry,
    string Currency,
    decimal CostUsdOrigin,
    decimal PercentageCommission,
    decimal FixedCommission,
    decimal ShippingCost,
    decimal TotalCostUsd,
    decimal TotalCostArs,
    string MarginType,
    decimal MarginValue,
    decimal SuggestedPriceArs,
    decimal CurrentPriceArs,
    bool HasPriceDifference,
    decimal ProfitArs,
    DateTime UpdatedAt,
    // Solo decants
    decimal? BottleCostArs,
    int? BottleMl,
    decimal? CostPerMl,
    List<DecantVariantPricingDto> DecantVariants
);

public record PricingCalculatorOverviewDto(PricingSettingsDto Settings, List<ProductPricingItemDto> Items);
