using ChipoBackend.Application.Features.Reports.Commands.ExportReport;
using ChipoBackend.Application.Features.Reports.DTOs;
using ChipoBackend.Application.Features.Reports.Queries.GetExpensesReport;
using ChipoBackend.Application.Features.Reports.Queries.GetFinancialReport;
using ChipoBackend.Application.Features.Reports.Queries.GetInventoryReport;
using ChipoBackend.Application.Features.Reports.Queries.GetPurchasesReport;
using ChipoBackend.Application.Features.Reports.Queries.GetSalesReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Finance,Supervisor")]
public class ReportsController : BaseApiController
{

    /// <summary>Defaults: últimos 30 días; "to" se extiende a fin de día para incluir registros del mismo día.</summary>
    private static (DateTime From, DateTime To) NormalizeRange(DateTime? from, DateTime? to)
    {
        var f = from ?? DateTime.UtcNow.AddDays(-30);
        var t = to ?? DateTime.UtcNow;
        if (t.TimeOfDay == TimeSpan.Zero) t = t.AddDays(1).AddTicks(-1);
        return (f, t);
    }

    /// <summary>Reporte de ventas</summary>
    [HttpGet("sales")]
    public async Task<IActionResult> GetSales(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var (f, t) = NormalizeRange(from, to);
        var result = await Mediator.Send(new GetSalesReportQuery(f, t, customerId, channel, search), ct);
        return Ok(result);
    }

    /// <summary>Reporte de inventario</summary>
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetInventoryReportQuery(categoryId, status), ct);
        return Ok(result);
    }

    /// <summary>Reporte de compras</summary>
    [HttpGet("purchases")]
    public async Task<IActionResult> GetPurchases(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? supplierId = null,
        CancellationToken ct = default)
    {
        var (f, t) = NormalizeRange(from, to);
        var result = await Mediator.Send(new GetPurchasesReportQuery(f, t, supplierId), ct);
        return Ok(result);
    }

    /// <summary>Reporte de gastos</summary>
    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenses(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var (f, t) = NormalizeRange(from, to);
        var result = await Mediator.Send(new GetExpensesReportQuery(f, t, categoryId, status), ct);
        return Ok(result);
    }

    /// <summary>Reporte financiero consolidado</summary>
    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancial(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? groupBy = "day",
        CancellationToken ct = default)
    {
        var (f, t) = NormalizeRange(from, to);
        var result = await Mediator.Send(new GetFinancialReportQuery(f, t, groupBy), ct);
        return Ok(result);
    }

    /// <summary>Exportar reporte en CSV, XLSX o PDF</summary>
    [HttpPost("export")]
    public async Task<IActionResult> Export([FromBody] ExportRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new ExportReportCommand(request), ct);
        return File(result.FileBytes, result.ContentType, result.FileName);
    }
}
