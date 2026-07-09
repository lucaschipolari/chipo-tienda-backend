using ChipoBackend.Application.Features.Suppliers.Commands.ChangeSupplierStatus;
using ChipoBackend.Application.Features.Suppliers.Commands.CreateSupplier;
using ChipoBackend.Application.Features.Suppliers.Commands.UpdateSupplier;
using ChipoBackend.Application.Features.Suppliers.Queries.GetSupplierById;
using ChipoBackend.Application.Features.Suppliers.Queries.GetSuppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
public class SuppliersController : BaseApiController
{
    /// <summary>Listado paginado de proveedores con filtros</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetSuppliersQuery(page, pageSize, search, isActive), ct);
        return Ok(result);
    }

    /// <summary>Detalle completo de un proveedor (con contactos)</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSupplierByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Crear un proveedor nuevo</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Actualizar datos de un proveedor</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("El ID de la ruta no coincide con el del body.");
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Activar o desactivar un proveedor</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ChangeStatus(
        Guid id, [FromBody] SupplierStatusRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ChangeSupplierStatusCommand(id, request.IsActive), ct);
        return NoContent();
    }
}

public record SupplierStatusRequest(bool IsActive);
