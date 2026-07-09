using ChipoBackend.Application.Features.Orders.Commands.ChangeOrderStatus;
using ChipoBackend.Application.Features.Orders.Commands.CreateOrder;
using ChipoBackend.Application.Features.Orders.Queries.GetOrderById;
using ChipoBackend.Application.Features.Orders.Queries.GetOrderByNumber;
using ChipoBackend.Application.Features.Orders.Queries.GetOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
public class OrdersController : BaseApiController
{
    /// <summary>Listado paginado de pedidos con filtros avanzados</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor,Vendedor")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null,
        [FromQuery] string? email = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetOrdersQuery(page, pageSize, customerId, status, from, to, search, email), ct);
        return Ok(result);
    }

    /// <summary>Detalle completo de un pedido por ID</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor,Vendedor")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrderByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Pedidos del usuario autenticado (cualquier rol) — busca por su email</summary>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrWhiteSpace(email)) return Forbid();

        var result = await Mediator.Send(new GetOrdersQuery(page, pageSize, Email: email), ct);
        return Ok(result);
    }

    /// <summary>Detalle de un pedido propio — valida que el pedido pertenezca al usuario</summary>
    [HttpGet("mine/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetMineById(Guid id, CancellationToken ct)
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? User.FindFirst("email")?.Value;
        if (string.IsNullOrWhiteSpace(email)) return Forbid();

        var result = await Mediator.Send(new GetOrderByIdQuery(id), ct);
        if (!string.Equals(result.BuyerEmail, email, StringComparison.OrdinalIgnoreCase))
            return Forbid();
        return Ok(result);
    }

    /// <summary>Detalle completo de un pedido por número de orden</summary>
    [HttpGet("number/{orderNumber}")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor,Vendedor")]
    public async Task<IActionResult> GetByOrderNumber(string orderNumber, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetOrderByNumberQuery(orderNumber), ct);
        return Ok(result);
    }

    /// <summary>Crear un pedido nuevo — soporta cliente registrado y guest checkout</summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>
    /// Cambiar estado del pedido.
    /// Reglas: Confirmed→descuenta stock | Cancelled→restaura stock (si estaba confirmado)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> ChangeStatus(
        Guid id, [FromBody] OrderStatusChangeRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ChangeOrderStatusCommand(id, request.NewStatus, request.Note), ct);
        return NoContent();
    }
}

public record OrderStatusChangeRequest(string NewStatus, string? Note = null);
