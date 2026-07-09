using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Customers;

public class Customer : AuditableEntity
{
    public Guid? UserId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }

    // Documento
    public string DocumentNumber { get; private set; } = null!;
    public DocumentType DocumentType { get; private set; } = DocumentType.DNI;

    // Dirección principal
    public string? Street { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }
    public string? PostalCode { get; private set; }

    public CustomerType CustomerType { get; private set; } = CustomerType.Retail;
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    public string FullName => $"{FirstName} {LastName}";

    private readonly List<CustomerAddress> _addresses = [];
    public IReadOnlyCollection<CustomerAddress> Addresses => _addresses.AsReadOnly();

    private Customer() { }

    public static Customer Create(
        string firstName, string lastName,
        string documentNumber, DocumentType documentType,
        string? email = null, string? phoneNumber = null,
        Guid? userId = null)
    {
        return new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            DocumentNumber = documentNumber,
            DocumentType = documentType,
            Email = email,
            PhoneNumber = phoneNumber,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Update(
        string firstName, string lastName,
        string documentNumber, DocumentType documentType,
        string? email, string? phoneNumber,
        string? street, string? city, string? province, string? postalCode,
        CustomerType type, string? notes)
    {
        FirstName = firstName;
        LastName = lastName;
        DocumentNumber = documentNumber;
        DocumentType = documentType;
        Email = email;
        PhoneNumber = phoneNumber;
        Street = street;
        City = city;
        Province = province;
        PostalCode = postalCode;
        CustomerType = type;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public CustomerAddress AddAddress(string label, Address address, bool isDefault = false)
    {
        if (isDefault)
            foreach (var a in _addresses) a.SetAsNotDefault();

        var customerAddress = CustomerAddress.Create(Id, label, address, isDefault);
        _addresses.Add(customerAddress);
        return customerAddress;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
}

public enum CustomerType   { Retail, Wholesale }
public enum DocumentType   { DNI, RUC, CE, Pasaporte }
