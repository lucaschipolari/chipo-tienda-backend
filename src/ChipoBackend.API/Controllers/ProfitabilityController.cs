using ChipoBackend.Application.Features.Profitability;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
public class ProfitabilityController : BaseApiController
{
    /// <summary>Tabla general de rentabilidad con filtros y orden.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? categoryId,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] bool? costIncreased,
        [FromQuery] bool? belowSuggested,
        [FromQuery] decimal? minMargin,
        [FromQuery] decimal? maxMargin,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        CancellationToken ct)
        => Ok(await Mediator.Send(new GetProfitabilityQuery(
            categoryId, status, search, costIncreased, belowSuggested, minMargin, maxMargin, sortBy, sortDir), ct));

    /// <summary>Tarjetas resumen (alertas) del tablero.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
        => Ok(await Mediator.Send(new GetProfitabilitySummaryQuery(), ct));

    /// <summary>Análisis detallado + historial de costos de un producto.</summary>
    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetDetail(Guid productId, CancellationToken ct)
    {
        var detail = await Mediator.Send(new GetProductProfitabilityQuery(productId), ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    /// <summary>Define/limpia el margen objetivo específico de un producto.</summary>
    [HttpPut("{productId:guid}/target-margin")]
    public async Task<IActionResult> SetTargetMargin(Guid productId, [FromBody] SetTargetMarginRequest request, CancellationToken ct)
    {
        await Mediator.Send(new SetProductTargetMarginCommand(productId, request.TargetMarginPct), ct);
        return NoContent();
    }

    /// <summary>Configuración global de rentabilidad (margen general, banda crítica, redondeo).</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
        => Ok(await Mediator.Send(new GetProfitabilitySettingsQuery(), ct));

    [HttpPut("settings")]
    public async Task<IActionResult> SetSettings([FromBody] ProfitabilitySettingsDto settings, CancellationToken ct)
    {
        await Mediator.Send(new SetProfitabilitySettingsCommand(settings), ct);
        return NoContent();
    }
}

public record SetTargetMarginRequest(decimal? TargetMarginPct);
