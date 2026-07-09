using ChipoBackend.Application.Features.Customers.Commands.ChangeCustomerStatus;
using ChipoBackend.Application.Features.Customers.Commands.CreateCustomer;
using ChipoBackend.Application.Features.Customers.Commands.UpdateCustomer;
using ChipoBackend.Application.Features.Customers.Queries.GetCustomerById;
using ChipoBackend.Application.Features.Customers.Queries.GetCustomers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Supervisor,Vendedor")]
public class CustomersController : BaseApiController
{
    /// <summary>Listado paginado de clientes con estadísticas básicas</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCustomersQuery(page, pageSize, search, isActive), ct);
        return Ok(result);
    }

    /// <summary>Detalle completo del cliente con historial de pedidos</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Crear un nuevo cliente</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Actualizar datos de un cliente</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("El ID de la ruta no coincide con el del body.");
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Activar o desactivar un cliente (borrado lógico)</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] CustomerStatusRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ChangeCustomerStatusCommand(id, request.IsActive), ct);
        return NoContent();
    }
}

public record CustomerStatusRequest(bool IsActive);
