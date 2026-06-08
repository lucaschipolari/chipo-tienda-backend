using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Users.DTOs;
using MediatR;

namespace ChipoBackend.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<PagedResult<UserListItemDto>>;
