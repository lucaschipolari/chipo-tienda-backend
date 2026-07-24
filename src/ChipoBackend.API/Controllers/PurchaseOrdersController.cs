using ChipoBackend.Application.Features.PurchaseOrders.Commands.ApprovePurchaseOrder;
using ChipoBackend.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;
using ChipoBackend.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;
using ChipoBackend.Application.Features.PurchaseOrders.Commands.DeletePurchaseOrder;
using ChipoBackend.Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder;
using ChipoBackend.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;
using ChipoBackend.Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;
using ChipoBackend.Application.Features.PurchaseOrders.DTOs;
using ChipoBackend.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;
using ChipoBackend.Application.Features.PurchaseOrders.Queries.GetPurchaseOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/purchase-orders")]
[Authorize(Roles = "SuperAdmin,Admin,Supervisor,Almacen")]
public class PurchaseOrdersController : BaseApiController
{
    /// <summary>Listado paginado de órdenes de compra con filtros</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetPurchaseOrdersQuery(page, pageSize, supplierId, status), ct);
        return Ok(result);
    }

    /// <summary>Detalle completo de una orden de compra (con ítems)</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPurchaseOrderByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Crear una orden de compra en estado Borrador</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Editar una orden de compra en estado Borrador</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("El ID de la ruta no coincide con el del body.");
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Enviar la orden al proveedor (Draft → Sent)</summary>
    [HttpPost("{id:guid}/send")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Send(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new SendPurchaseOrderCommand(id), ct);
        return NoContent();
    }

    /// <summary>Aprobar la orden (Sent → Approved)</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new ApprovePurchaseOrderCommand(id), ct);
        return NoContent();
    }

    /// <summary>Registrar recepción parcial o total de ítems</summary>
    [HttpPost("{id:guid}/receive")]
    [Authorize(Roles = "SuperAdmin,Admin,Almacen")]
    public async Task<IActionResult> Receive(
        Guid id, [FromBody] ReceiveItemsRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ReceivePurchaseOrderCommand(id, request.ItemReceipts), ct);
        return NoContent();
    }

    /// <summary>Eliminar físicamente una orden (solo Admin) — para limpiar pruebas.</summary>
    [HttpDelete("{id:guid}/permanent")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeletePermanent(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeletePurchaseOrderCommand(id), ct);
        return NoContent();
    }

    /// <summary>Cancelar una orden (solo si está en estado Draft)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new CancelPurchaseOrderCommand(id), ct);
        return NoContent();
    }
}
