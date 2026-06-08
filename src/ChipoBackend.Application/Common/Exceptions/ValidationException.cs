namespace ChipoBackend.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Se produjeron uno o más errores de validación.")
    {
        Errors = errors;
    }
}
