namespace ChipoBackend.Application.Features.Combos.DTOs;

public record ComboItemRequest(Guid ProductId, Guid VariantId, int Quantity);

public record ComboItemDto(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string VariantLabel,
    int Quantity,
    decimal UnitPrice,       // precio normal de la variante
    bool IsDecant,
    string? ImageUrl
);

public record ComboDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    decimal Price,           // precio del combo
    decimal OriginalTotal,   // suma de precios normales (para mostrar el ahorro)
    string Currency,
    bool IsActive,
    int ItemCount,
    IReadOnlyList<ComboItemDto> Items,
    DateTime CreatedAt
);
