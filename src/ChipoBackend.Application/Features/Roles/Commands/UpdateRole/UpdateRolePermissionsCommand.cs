using MediatR;

namespace ChipoBackend.Application.Features.Roles.Commands.UpdateRole;

public record UpdateRolePermissionsCommand(Guid RoleId, IReadOnlyList<Guid> PermissionIds) : IRequest;
