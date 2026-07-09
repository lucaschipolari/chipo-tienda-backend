using ChipoBackend.Application.Features.Inventory.Commands.AdjustStock;
using ChipoBackend.Application.Features.Inventory.Queries.GetLowStock;
using ChipoBackend.Application.Features.Inventory.Queries.GetStockMovements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Supervisor,Almacen")]
public class InventoryController : BaseApiController
{
    /// <summary>Historial de movimientos de stock (paginado, filtrable por producto/variante)</summary>
    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? variantId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetStockMovementsQuery(productId, variantId, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Productos / variantes con stock por debajo del umbral mínimo</summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetLowStockQuery(), ct);
        return Ok(result);
    }

    /// <summary>Ajuste manual de stock de una variante</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }
}
