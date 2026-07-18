using ChipoBackend.Application.Features.Analytics.Commands.RecordEvent;
using ChipoBackend.Application.Features.Analytics.Queries.GetDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/analytics")]
public class AnalyticsController : BaseApiController
{
    /// <summary>Registrar un evento de interacción anónimo (desde la tienda).</summary>
    [HttpPost("event")]
    [AllowAnonymous]
    public async Task<IActionResult> Record([FromBody] RecordAnalyticsEventCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Dashboard de analítica (rankings + resumen). Solo administración.</summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> Dashboard(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var t = to ?? DateTime.UtcNow;
        var f = from ?? t.AddDays(-30);
        var result = await Mediator.Send(new GetAnalyticsDashboardQuery(f, t), ct);
        return Ok(result);
    }
}
