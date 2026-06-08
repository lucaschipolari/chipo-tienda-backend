using MediatR;

namespace ChipoBackend.Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(string Name, string? Description) : IRequest<Guid>;
