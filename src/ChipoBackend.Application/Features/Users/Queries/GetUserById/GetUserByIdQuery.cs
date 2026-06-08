using ChipoBackend.Application.Features.Users.DTOs;
using MediatR;

namespace ChipoBackend.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
