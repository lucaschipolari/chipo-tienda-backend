using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Users;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateRoleCommand, Guid>
{
    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        if (await roleRepository.ExistsAsync(r => r.Name == request.Name, ct))
            throw new ConflictException($"El rol '{request.Name}' ya existe.");

        var role = Role.Create(request.Name, request.Description);
        await roleRepository.AddAsync(role, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return role.Id;
    }
}
