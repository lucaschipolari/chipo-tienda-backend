using System.Text.RegularExpressions;
using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Exceptions;

namespace ChipoBackend.Domain.ValueObjects;

public sealed class EmailAddress : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private EmailAddress() { Value = ""; }

    private EmailAddress(string value) => Value = value.ToLowerInvariant().Trim();

    public static EmailAddress Of(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El email no puede estar vacío.");
        if (!EmailRegex.IsMatch(email))
            throw new DomainException($"El email '{email}' no tiene un formato válido.");
        return new EmailAddress(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
    public static implicit operator string(EmailAddress email) => email.Value;
}
