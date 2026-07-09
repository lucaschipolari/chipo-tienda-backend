using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Audit;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeTokenCommand>
{
    public async Task Handle(RevokeTokenCommand request, CancellationToken ct)
    {
        // GetByRefreshTokenAsync busca por hash en la BD.
        // El cliente envía el token crudo — hay que hashear antes de consultar.
        var tokenHash = jwtService.HashRefreshToken(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenAsync(tokenHash, ct)
            ?? throw new NotFoundException("Token no encontrado.");

        var token = user.RefreshTokens.FirstOrDefault(t =>
            jwtService.ValidateRefreshTokenHash(request.RefreshToken, t.TokenHash))
            ?? throw new NotFoundException("Token no encontrado.");

        if (!token.IsActive)
            throw new ForbiddenException("El token ya fue revocado o expiró.");

        user.RevokeRefreshToken(token.TokenHash, "logout");
        await unitOfWork.SaveChangesAsync(ct);
        await auditService.LogAsync(AuditAction.Logout, "User", user.Id.ToString(), ct: ct);
    }
}
