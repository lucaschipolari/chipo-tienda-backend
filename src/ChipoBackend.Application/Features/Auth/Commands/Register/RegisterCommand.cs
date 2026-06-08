using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string FirstName, string LastName, string? PhoneNumber) : IRequest<RegisterResponse>;

public record RegisterResponse(Guid UserId, string Email, string FullName);
