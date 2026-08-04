using ChipoBackend.Application.Features.Combos.Commands.CreateCombo;
using ChipoBackend.Application.Features.Combos.Commands.DeleteCombo;
using ChipoBackend.Application.Features.Combos.Commands.ToggleComboStatus;
using ChipoBackend.Application.Features.Combos.Commands.UpdateCombo;
using ChipoBackend.Application.Features.Combos.Queries.GetComboById;
using ChipoBackend.Application.Features.Combos.Queries.GetCombos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/combos")]
public class CombosController : BaseApiController
{
    /// <summary>Listado de combos. Público: por defecto solo activos.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCombosQuery(onlyActive), ct);
        return Ok(result);
    }

    /// <summary>Todos los combos (activos e inactivos) — para el panel.</summary>
    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor,Vendedor")]
    public async Task<IActionResult> GetAllAdmin(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCombosQuery(false), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetComboByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateComboCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateComboCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("El ID de la ruta no coincide con el del body.");
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] ComboStatusRequest body, CancellationToken ct)
    {
        await Mediator.Send(new ToggleComboStatusCommand(id, body.IsActive), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteComboCommand(id), ct);
        return NoContent();
    }
}

public record ComboStatusRequest(bool IsActive);
