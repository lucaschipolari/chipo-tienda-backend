using ChipoBackend.Application.Features.Sales.Commands.CreateSale;
using ChipoBackend.Application.Features.Sales.Queries.GetSaleById;
using ChipoBackend.Application.Features.Sales.Queries.GetSales;
using ChipoBackend.Application.Features.Sales.Queries.GetSalesReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Supervisor,Vendedor")]
public class SalesController : BaseApiController
{
    /// <summary>Listado paginado de ventas</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetSalesQuery(page, pageSize, customerId, from, to), ct);
        return Ok(result);
    }

    /// <summary>Detalle de una venta</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSaleByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Registrar una venta directa (descuenta stock inmediatamente)</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Reporte de ventas por período con métricas y gráficas</summary>
    [HttpGet("report")]
    [Authorize(Roles = "SuperAdmin,Admin,Finance")]
    public async Task<IActionResult> GetReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSalesReportQuery(from, to), ct);
        return Ok(result);
    }
}
