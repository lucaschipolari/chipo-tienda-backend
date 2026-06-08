using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRolePermissionsCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRolePermissionsCommand>
{
    public async Task Handle(UpdateRolePermissionsCommand request, CancellationToken ct)
    {
        var role = await roleRepository.GetWithPermissionsAsync(request.RoleId, ct)
            ?? throw new NotFoundException("Role", request.RoleId);

        if (role.IsSystem)
            throw new ForbiddenException("No se pueden modificar los permisos de un rol del sistema.");

        role.SetPermissions(request.PermissionIds);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
