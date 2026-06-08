using MediatR;

namespace ChipoBackend.Application.Features.Users.Commands.ChangeUserStatus;

public record ChangeUserStatusCommand(Guid UserId, string Action) : IRequest;
