using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserDto>;

public record CurrentUserDto(
    Guid Id,
    string Email,
    string FullName,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Status,
    IReadOnlyList<string> Roles,
    DateTime? LastLoginAt);

public class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var userId = currentUserService.UserId
            ?? throw new ForbiddenException("No autenticado.");

        var user = await userRepository.GetWithRolesAsync(userId, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        var roles = user.Roles.Select(ur => ur.Role.Name).ToList();

        return new CurrentUserDto(
            user.Id,
            user.Email.Value,
            user.FullName,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.Status.ToString(),
            roles,
            user.LastLoginAt);
    }
}
