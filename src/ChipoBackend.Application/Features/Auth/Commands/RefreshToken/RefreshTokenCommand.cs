using MediatR;
using ChipoBackend.Application.Features.Auth.Commands.Login;

namespace ChipoBackend.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<LoginResponse>;
