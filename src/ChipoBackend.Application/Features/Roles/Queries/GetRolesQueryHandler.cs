using ChipoBackend.Application.Features.Roles.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Roles.Queries;

public record GetRolesQuery : IRequest<IReadOnlyList<RoleListItemDto>>;
public record GetPermissionsQuery : IRequest<IReadOnlyList<PermissionDto>>;

public class GetRolesQueryHandler(IRoleRepository roleRepository)
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleListItemDto>>
{
    public async Task<IReadOnlyList<RoleListItemDto>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        var roles = await roleRepository.GetAllAsync(ct);
        return roles.Select(r => new RoleListItemDto(r.Id, r.Name, r.Description, r.IsSystem, r.Permissions.Count)).ToList();
    }
}

public class GetPermissionsQueryHandler(IRoleRepository roleRepository)
    : IRequestHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<IReadOnlyList<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken ct)
    {
        var permissions = await roleRepository.GetAllPermissionsAsync(ct);
        return permissions.Select(p => new PermissionDto(p.Id, p.Resource, p.Action, p.FullName, p.Description)).ToList();
    }
}
