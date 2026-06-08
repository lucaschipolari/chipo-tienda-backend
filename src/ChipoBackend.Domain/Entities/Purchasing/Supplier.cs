using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Purchasing;

public class Supplier : AuditableEntity
{
    public string CompanyName { get; private set; } = null!;
    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? TaxId { get; private set; }
    public Address? Address { get; private set; }
    public string? PaymentTerms { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Notes { get; private set; }

    private Supplier() { }

    public static Supplier Create(string companyName, string? contactName = null, string? email = null, string? phone = null)
    {
        return new Supplier
        {
            CompanyName = companyName,
            ContactName = contactName,
            Email = email,
            Phone = phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string companyName, string? contactName, string? email, string? phone, string? taxId, Address? address, string? paymentTerms, string? notes)
    {
        CompanyName = companyName;
        ContactName = contactName;
        Email = email;
        Phone = phone;
        TaxId = taxId;
        Address = address;
        PaymentTerms = paymentTerms;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
}
