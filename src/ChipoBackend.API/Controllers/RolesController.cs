using ChipoBackend.Application.Features.Roles.Commands.CreateRole;
using ChipoBackend.Application.Features.Roles.Commands.UpdateRole;
using ChipoBackend.Application.Features.Roles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class RolesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
        => Ok(await Mediator.Send(new GetRolesQuery(), ct));

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions(CancellationToken ct)
        => Ok(await Mediator.Send(new GetPermissionsQuery(), ct));

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var id = await Mediator.Send(new CreateRoleCommand(request.Name, request.Description), ct);
        return CreatedAtAction(nameof(GetRoles), new { id }, new { id });
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] UpdatePermissionsRequest request, CancellationToken ct)
    {
        await Mediator.Send(new UpdateRolePermissionsCommand(id, request.PermissionIds), ct);
        return NoContent();
    }
}

public record CreateRoleRequest(string Name, string? Description);
public record UpdatePermissionsRequest(IReadOnlyList<Guid> PermissionIds);
