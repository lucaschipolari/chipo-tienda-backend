namespace ChipoBackend.Application.Features.Analytics.DTOs;

public record ProductStatDto(
    Guid ProductId,
    string ProductName,
    string? CategoryName,
    int Count,
    int Views,          // para calcular conversión donde aplique
    double? ConversionRate // Count/Views (%) — solo en el ranking de carrito
);

public record SearchStatDto(string Term, int Count, int NoResultCount);

public record AnalyticsSummaryDto(
    int TotalViews,
    int TotalAddToCart,
    int TotalFavorites,
    int TotalSearches,
    int UniqueVisitors,
    int UniqueProductsViewed,
    double ViewToCartRate,          // % vistas que terminaron en carrito
    double ViewsVsPreviousPeriod    // variación % vs período anterior
);

public record AnalyticsDashboardDto(
    DateTime From,
    DateTime To,
    AnalyticsSummaryDto Summary,
    IReadOnlyList<ProductStatDto> TopViewed,
    IReadOnlyList<ProductStatDto> TopAddedToCart,
    IReadOnlyList<ProductStatDto> TopFavorited,
    IReadOnlyList<SearchStatDto> TopSearches,
    IReadOnlyList<SearchStatDto> NoResultSearches
);
