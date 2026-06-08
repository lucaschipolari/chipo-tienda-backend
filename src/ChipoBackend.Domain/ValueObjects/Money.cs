using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Exceptions;

namespace ChipoBackend.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { Amount = 0; Currency = "PEN"; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Of(decimal amount, string currency = "PEN")
    {
        if (amount < 0)
            throw new DomainException("El monto no puede ser negativo.");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("La moneda es requerida.");
        return new Money(amount, currency);
    }

    public static Money Zero(string currency = "PEN") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"No se pueden sumar montos de distinta moneda: {Currency} y {other.Currency}.");
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"No se pueden restar montos de distinta moneda.");
        if (Amount < other.Amount)
            throw new DomainException("El monto resultante no puede ser negativo.");
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Math.Round(Amount * factor, 2), Currency);

    public static Money operator +(Money a, Money b) => a.Add(b);
    public static Money operator -(Money a, Money b) => a.Subtract(b);
    public static Money operator *(Money a, decimal factor) => a.Multiply(factor);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:F2}";
}
