using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.Auth.Commands.Login;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var (_, hash, _) = jwtService.GenerateRefreshToken(); // use validation only
        var user = await userRepository.GetByRefreshTokenAsync(request.RefreshToken, ct)
            ?? throw new NotFoundException("Token de refresco inválido.");

        var existingToken = user.RefreshTokens.FirstOrDefault(t => jwtService.ValidateRefreshTokenHash(request.RefreshToken, t.TokenHash));
        if (existingToken == null || !existingToken.IsActive)
            throw new ForbiddenException("El token de refresco ha expirado o fue revocado.");

        var roles = user.Roles.Select(ur => ur.Role.Name).ToList();
        var accessToken = jwtService.GenerateAccessToken(user, roles);
        var (newRefreshTokenValue, newRefreshTokenHash, expiresAt) = jwtService.GenerateRefreshToken();

        user.RevokeRefreshToken(existingToken.TokenHash, "replaced");
        user.AddRefreshToken(newRefreshTokenHash, expiresAt, request.IpAddress);

        await unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse(accessToken, newRefreshTokenValue, expiresAt,
            new UserInfo(user.Id, user.Email.Value, user.FullName, roles));
    }
}
