using ChipoBackend.Application.Features.Users.Commands.AssignRole;
using ChipoBackend.Application.Features.Users.Commands.ChangeUserStatus;
using ChipoBackend.Application.Features.Users.Queries.GetUserById;
using ChipoBackend.Application.Features.Users.Queries.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        => Ok(await Mediator.Send(new GetUsersQuery(page, pageSize, search), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
        => Ok(await Mediator.Send(new GetUserByIdQuery(id), ct));

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ChangeUserStatusCommand(id, request.Action), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] RoleRequest request, CancellationToken ct)
    {
        await Mediator.Send(new AssignRoleCommand(id, request.RoleId), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    public async Task<IActionResult> RemoveRole(Guid id, Guid roleId, CancellationToken ct)
    {
        await Mediator.Send(new AssignRoleCommand(id, roleId, Remove: true), ct);
        return NoContent();
    }
}

public record ChangeStatusRequest(string Action);
public record RoleRequest(Guid RoleId);
