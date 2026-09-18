namespace ChipoBackend.Application.Features.Profitability;

/// <summary>Una fila de la tabla general de rentabilidad (una por variante).</summary>
public record ProfitabilityRowDto(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    Guid CategoryId,
    string? Brand,          // hoy = categoría
    string Sku,
    string? VariantLabel,   // atributos, ej "100ml"
    bool IsDecant,
    string Currency,
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

/// <summary>Tarjetas resumen del tablero (tu #6).</summary>
public record ProfitabilitySummaryDto(
    int TotalAnalyzed,
    int Optimo,
    int Bajo,
    int Critico,
    int SinDatos,
    int CostIncreased,      // productos cuyo costo aumentó
    int NeedsPriceReview    // por debajo del objetivo o precio < sugerido
);

/// <summary>Un registro del historial de costos de un producto/variante.</summary>
public record CostHistoryEntryDto(
    Guid Id,
    decimal UnitCost,
    string Currency,
    string Source,
    Guid? PurchaseOrderId,
    DateTime RecordedAt,
    decimal? ChangeAbs,
    decimal? ChangePct
);

/// <summary>Detalle completo de un producto para la vista de análisis (tu #12).</summary>
public record ProductProfitabilityDetailDto(
    ProfitabilityRowDto Analysis,
    List<CostHistoryEntryDto> History
);
