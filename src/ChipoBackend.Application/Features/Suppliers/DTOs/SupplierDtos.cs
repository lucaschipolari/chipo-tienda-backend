namespace ChipoBackend.Application.Features.Suppliers.DTOs;

public record SupplierContactDto(
    Guid Id,
    string Name,
    string? JobTitle,
    string? Email,
    string? Phone,
    bool IsPrimary
);

public record SupplierProductDto(
    Guid ProductId,
    string? ProductName,
    string? SupplierProductCode,
    decimal PurchasePrice,
    string Currency,
    int LeadTimeDays,
    bool IsPreferredSupplier
);

public record SupplierDto(
    Guid Id,
    string CompanyName,
    string? TradeName,
    string? TaxId,
    string? Email,
    string? Phone,
    string? Website,
    string? City,
    string? Province,
    string? Country,
    string? PaymentTerms,
    bool IsActive,
    string? Notes,
    List<SupplierContactDto> Contacts,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record SupplierListItemDto(
    Guid Id,
    string CompanyName,
    string? TradeName,
    string? TaxId,
    string? Email,
    string? Phone,
    string? City,
    bool IsActive,
    int ProductCount,
    DateTime CreatedAt
);

public record CreateSupplierRequest(
    string CompanyName,
    string? TradeName,
    string? TaxId,
    string? Email,
    string? Phone,
    string? Website,
    string? City,
    string? Province,
    string? Country,
    string? PaymentTerms,
    string? Notes
);

public record UpdateSupplierRequest(
    string CompanyName,
    string? TradeName,
    string? TaxId,
    string? Email,
    string? Phone,
    string? Website,
    string? City,
    string? Province,
    string? Country,
    string? PaymentTerms,
    string? Notes
);
