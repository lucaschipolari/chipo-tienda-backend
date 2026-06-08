using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Users.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await userRepository.GetWithRolesAsync(request.Id, ct)
            ?? throw new NotFoundException("User", request.Id);

        return new UserDto(
            user.Id, user.Email.Value, user.FirstName, user.LastName, user.PhoneNumber,
            user.Status.ToString(), user.IsEmailConfirmed, user.LastLoginAt,
            user.Roles.Select(r => r.Role?.Name ?? "").ToList(),
            user.CreatedAt);
    }
}
