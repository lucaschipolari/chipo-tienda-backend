namespace ChipoBackend.Application.Features.Products.DTOs;

/// <summary>Perfil olfativo — usado tanto en input (crear/editar) como en output (detalle).</summary>
public record OlfactoryProfileDto(
    List<string> TopNotes,
    List<string> HeartNotes,
    List<string> BaseNotes,
    int? Intensity,
    string? Longevity,
    List<string> Seasons,
    List<string> Occasions);

public record ProductVariantDto(
    Guid Id,
    Guid ProductId,
    string Sku,
    string? Barcode,
    Dictionary<string, string> Attributes,
    decimal? Price,
    decimal? CompareAtPrice,
    string Currency,
    int StockQuantity,
    int MinStockThreshold,
    bool IsActive,
    bool IsBelowMinStock,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProductImageDto(
    Guid Id,
    string Url,
    string? AltText,
    int DisplayOrder);

public record ProductDto(
    Guid Id,
    Guid CategoryId,
    string? CategoryName,
    string Name,
    string Slug,
    string? Description,
    string Sku,
    decimal BasePrice,
    decimal? CompareAtPrice,
    string Currency,
    string Status,
    bool IsFeatured,
    List<string> Tags,
    OlfactoryProfileDto Olfactory,
    IReadOnlyList<ProductVariantDto> Variants,
    IReadOnlyList<ProductImageDto> Images,
    int TotalStock,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProductListItemDto(
    Guid Id,
    Guid CategoryId,
    string? CategoryName,
    string Name,
    string Slug,
    string Sku,
    decimal BasePrice,
    decimal? CompareAtPrice,
    string Currency,
    string Status,
    bool IsFeatured,
    int TotalStock,
    int VariantCount,
    Guid? DefaultVariantId,
    int DefaultVariantStock,
    string? MainImageUrl,
    string? Description,
    IReadOnlyList<string> Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Request de variante para creación de producto</summary>
public record CreateVariantRequest(
    string Sku,
    Dictionary<string, string> Attributes,
    int InitialStock = 0,
    decimal? Price = null,
    int MinStockThreshold = 5,
    decimal? CompareAtPrice = null);
