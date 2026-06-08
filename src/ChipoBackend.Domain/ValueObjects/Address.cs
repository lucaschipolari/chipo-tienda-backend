using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string? PostalCode { get; }
    public string Country { get; }

    private Address() { Street = ""; City = ""; Country = "Peru"; }

    public Address(string street, string city, string? state, string? postalCode, string country = "Peru")
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
        yield return Country;
    }

    public override string ToString() => $"{Street}, {City}{(State != null ? ", " + State : "")}, {Country}";
}
