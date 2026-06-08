using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Customers;

public class CustomerAddress : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public string Label { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    private CustomerAddress() { }

    public static CustomerAddress Create(Guid customerId, string label, Address address, bool isDefault)
    {
        return new CustomerAddress { CustomerId = customerId, Label = label, Address = address, IsDefault = isDefault };
    }

    public void SetAsDefault() => IsDefault = true;
    public void SetAsNotDefault() => IsDefault = false;
}
