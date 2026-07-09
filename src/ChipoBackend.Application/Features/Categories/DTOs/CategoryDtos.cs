namespace ChipoBackend.Application.Features.Categories.DTOs;

public record CategoryDto(
    Guid Id,
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    int ProductCount,
    List<CategoryDto> SubCategories,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>Versión plana sin subcategorías, para selectores y dropdowns</summary>
public record CategoryFlatDto(
    Guid Id,
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    bool IsActive,
    int DisplayOrder);
