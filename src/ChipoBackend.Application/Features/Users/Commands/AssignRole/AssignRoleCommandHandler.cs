using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Users.Commands.AssignRole;

public class AssignRoleCommandHandler(IUserRepository userRepository, IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<AssignRoleCommand>
{
    public async Task Handle(AssignRoleCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        if (!await roleRepository.ExistsAsync(r => r.Id == request.RoleId, ct))
            throw new NotFoundException("Role", request.RoleId);

        if (request.Remove) user.RemoveRole(request.RoleId);
        else user.AssignRole(request.RoleId);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
