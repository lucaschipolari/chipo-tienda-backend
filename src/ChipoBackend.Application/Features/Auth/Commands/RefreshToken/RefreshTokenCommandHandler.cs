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
        // GetByRefreshTokenAsync busca por hash en la BD.
        // El cliente envía el token crudo — hay que hashear antes de consultar.
        var tokenHash = jwtService.HashRefreshToken(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenAsync(tokenHash, ct)
            ?? throw new NotFoundException("Token de refresco inválido.");

        var existingToken = user.RefreshTokens.FirstOrDefault(t =>
            jwtService.ValidateRefreshTokenHash(request.RefreshToken, t.TokenHash));

        if (existingToken == null || !existingToken.IsActive)
            throw new ForbiddenException("El token de refresco ha expirado o fue revocado.");

        var roles = user.Roles.Select(ur => ur.Role.Name).ToList();
        var accessToken = jwtService.GenerateAccessToken(user, roles);
        var (newRefreshTokenValue, newRefreshTokenHash, expiresAt) = jwtService.GenerateRefreshToken();

        user.RevokeRefreshToken(existingToken.TokenHash, "replaced");
        var newToken = user.AddRefreshToken(newRefreshTokenHash, expiresAt, request.IpAddress);
        unitOfWork.Add(newToken);

        await unitOfWork.SaveChangesAsync(ct);

        return new LoginResponse(accessToken, newRefreshTokenValue, expiresAt,
            new UserInfo(user.Id, user.Email.Value, user.FullName, roles));
    }
}
