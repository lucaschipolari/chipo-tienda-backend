using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Audit;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (!passwordService.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ForbiddenException("La contraseña actual es incorrecta.");

        user.ChangePassword(passwordService.Hash(request.NewPassword));
        await unitOfWork.SaveChangesAsync(ct);
        await auditService.LogAsync(AuditAction.PasswordChange, "User", user.Id.ToString(), ct: ct);
    }
}
