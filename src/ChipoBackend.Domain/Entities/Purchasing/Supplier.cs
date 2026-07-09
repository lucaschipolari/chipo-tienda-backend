using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Purchasing;

public class Supplier : AuditableEntity
{
    public string CompanyName { get; private set; } = null!;
    public string? TradeName { get; private set; }
    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? TaxId { get; private set; }
    public string? Website { get; private set; }
    public Address? Address { get; private set; }
    public string? City { get; private set; }
    public string? Province { get; private set; }
    public string? Country { get; private set; } = "Peru";
    public string? PaymentTerms { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private readonly List<SupplierContact> _contacts = [];
    public IReadOnlyCollection<SupplierContact> Contacts => _contacts.AsReadOnly();

    private readonly List<SupplierProduct> _products = [];
    public IReadOnlyCollection<SupplierProduct> Products => _products.AsReadOnly();

    private Supplier() { }

    public static Supplier Create(
        string companyName,
        string? contactName = null,
        string? email = null,
        string? phone = null,
        string? tradeName = null,
        string? website = null,
        string? city = null,
        string? province = null,
        string? country = null)
    {
        return new Supplier
        {
            CompanyName = companyName,
            ContactName = contactName,
            Email = email,
            Phone = phone,
            TradeName = tradeName,
            Website = website,
            City = city,
            Province = province,
            Country = country ?? "Peru",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string companyName,
        string? contactName,
        string? email,
        string? phone,
        string? taxId,
        Address? address,
        string? paymentTerms,
        string? notes,
        string? tradeName = null,
        string? website = null,
        string? city = null,
        string? province = null,
        string? country = null)
    {
        CompanyName = companyName;
        ContactName = contactName;
        Email = email;
        Phone = phone;
        TaxId = taxId;
        Address = address;
        PaymentTerms = paymentTerms;
        Notes = notes;
        TradeName = tradeName;
        Website = website;
        City = city;
        Province = province;
        Country = country;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
}
