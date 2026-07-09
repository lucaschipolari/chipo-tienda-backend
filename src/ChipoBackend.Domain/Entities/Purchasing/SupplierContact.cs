using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Purchasing;

public class SupplierContact : BaseEntity
{
    public Guid SupplierId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? JobTitle { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsPrimary { get; private set; }

    private SupplierContact() { }

    public static SupplierContact Create(
        Guid supplierId,
        string name,
        string? jobTitle = null,
        string? email = null,
        string? phone = null,
        bool isPrimary = false)
    {
        return new SupplierContact
        {
            SupplierId = supplierId,
            Name = name,
            JobTitle = jobTitle,
            Email = email,
            Phone = phone,
            IsPrimary = isPrimary
        };
    }

    public void Update(
        string name,
        string? jobTitle,
        string? email,
        string? phone,
        bool isPrimary)
    {
        Name = name;
        JobTitle = jobTitle;
        Email = email;
        Phone = phone;
        IsPrimary = isPrimary;
    }
}
