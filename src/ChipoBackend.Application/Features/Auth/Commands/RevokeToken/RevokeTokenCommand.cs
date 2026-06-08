using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string RefreshToken) : IRequest;
