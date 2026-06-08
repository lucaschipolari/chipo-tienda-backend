using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress) : IRequest<LoginResponse>;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserInfo User);

public record UserInfo(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles);
