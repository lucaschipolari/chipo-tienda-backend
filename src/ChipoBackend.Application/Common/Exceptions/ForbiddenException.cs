namespace ChipoBackend.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "No tienes permisos para realizar esta acción.") : base(message) { }
}
