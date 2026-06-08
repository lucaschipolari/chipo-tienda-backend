using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Users.Commands.ChangeUserStatus;

public class ChangeUserStatusCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeUserStatusCommand>
{
    public async Task Handle(ChangeUserStatusCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        switch (request.Action.ToLower())
        {
            case "suspend": user.Suspend(); break;
            case "activate": user.Activate(); break;
            case "block": user.Block(); break;
            default: throw new ValidationException(new Dictionary<string, string[]> { { "action", ["Acción inválida. Use: suspend, activate, block."] } });
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
