using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Users.DTOs;
using ChipoBackend.Domain.Entities.Users;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;
using System.Linq.Expressions;

namespace ChipoBackend.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, PagedResult<UserListItemDto>>
{
    public async Task<PagedResult<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        Expression<Func<User, bool>> filter = request.Search == null
            ? _ => true
            : u => u.FirstName.Contains(request.Search) || u.LastName.Contains(request.Search) || u.Email.Value.Contains(request.Search);

        var users = await userRepository.FindAsync(filter, ct);
        var total = users.Count;
        var paged = users
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListItemDto(
                u.Id, u.Email.Value, u.FullName,
                u.Status.ToString(),
                u.Roles.Select(r => r.Role?.Name ?? "").ToList(),
                u.CreatedAt))
            .ToList();

        return PagedResult<UserListItemDto>.Create(paged, total, request.Page, request.PageSize);
    }
}
