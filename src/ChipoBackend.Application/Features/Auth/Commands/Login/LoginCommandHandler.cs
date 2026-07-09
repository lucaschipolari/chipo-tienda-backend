using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Audit;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct)
            ?? throw new NotFoundException("Credenciales inválidas.");

        if (!user.IsActive)
            throw new ForbiddenException("Tu cuenta está suspendida o bloqueada.");

        if (!passwordService.Verify(request.Password, user.PasswordHash))
            throw new NotFoundException("Credenciales inválidas.");

        var roles = user.Roles.Select(ur => ur.Role.Name).ToList();
        var accessToken = jwtService.GenerateAccessToken(user, roles);
        var (refreshTokenValue, refreshTokenHash, expiresAt) = jwtService.GenerateRefreshToken();

        user.RecordLogin();
        var refreshToken = user.AddRefreshToken(refreshTokenHash, expiresAt, request.IpAddress);

        // Registrar el RefreshToken explícitamente como Added.
        // Sin esto, EF Core lo detecta vía navigation fix-up como Modified (estado incorrecto)
        // porque RefreshTokens no fue cargado con Include(), generando un UPDATE en lugar de
        // INSERT y lanzando DbUpdateConcurrencyException (expected 1 row, affected 0 rows).
        unitOfWork.Add(refreshToken);

        await unitOfWork.SaveChangesAsync(ct);
        await auditService.LogAsync(AuditAction.Login, "User", user.Id.ToString(), ct: ct);

        return new LoginResponse(
            accessToken,
            refreshTokenValue,
            expiresAt,
            new UserInfo(user.Id, user.Email.Value, user.FullName, roles)
        );
    }
}
