namespace ChipoBackend.Application.Features.Customers.DTOs;

public record CustomerAddressDto(
    Guid Id,
    string Label,
    string Street,
    string City,
    string? State,
    string? PostalCode,
    string Country,
    bool IsDefault
);

public record CustomerDto(
    Guid Id,
    Guid? UserId,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string DocumentNumber,
    string DocumentType,
    string? Street,
    string? City,
    string? Province,
    string? PostalCode,
    string CustomerType,
    bool IsActive,
    string? Notes,
    List<CustomerAddressDto> Addresses,
    int TotalOrders,
    decimal TotalSpent,
    string Currency,
    DateTime? LastOrderAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CustomerListItemDto(
    Guid Id,
    string FullName,
    string? Email,
    string? PhoneNumber,
    string DocumentNumber,
    string DocumentType,
    string? City,
    string CustomerType,
    bool IsActive,
    int TotalOrders,
    decimal TotalSpent,
    string Currency,
    DateTime? LastOrderAt,
    DateTime CreatedAt
);
