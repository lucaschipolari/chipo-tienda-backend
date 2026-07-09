using ChipoBackend.Application.Features.Finance.Queries.GetCashFlow;
using ChipoBackend.Application.Features.Finance.Queries.GetFinanceDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/finance")]
[Authorize(Roles = "SuperAdmin,Admin,Finance")]
public class FinanceController : BaseApiController
{
    // GET api/finance/dashboard?period=month&from=&to=
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] string period = "month",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetFinanceDashboardQuery(from, to, period), ct);
        return Ok(result);
    }

    // GET api/finance/cash-flow?granularity=daily&from=&to=
    [HttpGet("cash-flow")]
    public async Task<IActionResult> GetCashFlow(
        [FromQuery] string granularity = "daily",
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCashFlowQuery(granularity, from, to), ct);
        return Ok(result);
    }
}
