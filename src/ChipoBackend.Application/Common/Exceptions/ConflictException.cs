namespace ChipoBackend.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
