using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Users.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, PagedResult<UserListItemDto>>
{
    public async Task<PagedResult<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        // GetPagedAsync aplica filtro en DB con EF.Property para el email
        // (evita el problema de HasConversion con u.Email.Value en LINQ-to-SQL)
        var (users, total) = await userRepository.GetPagedAsync(
            request.Search,
            request.Page,
            request.PageSize,
            ct);

        var dtos = users
            .Select(u => new UserListItemDto(
                u.Id,
                u.Email.Value,   // .Value se usa en memoria, no en SQL — seguro
                u.FullName,
                u.Status.ToString(),
                u.Roles.Select(r => r.Role?.Name ?? "").ToList(),
                u.CreatedAt))
            .ToList();

        return PagedResult<UserListItemDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
