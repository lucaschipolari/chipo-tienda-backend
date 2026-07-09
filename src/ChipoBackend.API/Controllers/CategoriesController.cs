using ChipoBackend.Application.Features.Categories.Commands.CreateCategory;
using ChipoBackend.Application.Features.Categories.Commands.DeleteCategory;
using ChipoBackend.Application.Features.Categories.Commands.UpdateCategory;
using ChipoBackend.Application.Features.Categories.Queries.GetCategories;
using ChipoBackend.Application.Features.Categories.Queries.GetCategoryById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
public class CategoriesController : BaseApiController
{
    // ── Queries ──────────────────────────────────────────────────────────────

    /// <summary>Árbol de categorías activas (público)</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCategoriesQuery(includeInactive), ct);
        return Ok(result);
    }

    /// <summary>Detalle de una categoría por ID</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCategoryByIdQuery(id), ct);
        return Ok(result);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>Crear una categoría nueva</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Actualizar una categoría existente</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("El ID de la ruta no coincide con el del body.");
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Desactivar (soft delete) una categoría sin productos</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteCategoryCommand(id), ct);
        return NoContent();
    }
}
