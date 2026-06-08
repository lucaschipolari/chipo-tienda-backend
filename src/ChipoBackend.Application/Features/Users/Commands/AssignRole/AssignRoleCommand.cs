using MediatR;

namespace ChipoBackend.Application.Features.Users.Commands.AssignRole;

public record AssignRoleCommand(Guid UserId, Guid RoleId, bool Remove = false) : IRequest;
