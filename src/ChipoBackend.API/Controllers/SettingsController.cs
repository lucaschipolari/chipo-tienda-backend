using ChipoBackend.Application.Features.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
public class SettingsController : BaseApiController
{
    /// <summary>Costo del frasquito (envase) por tamaño de decant — público para la tienda.</summary>
    [HttpGet("vial-costs")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVialCosts(CancellationToken ct)
        => Ok(await Mediator.Send(new GetVialCostsQuery(), ct));

    /// <summary>Actualiza el costo de frasquitos por tamaño (aplica a todos los decants).</summary>
    [HttpPut("vial-costs")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> SetVialCosts([FromBody] SetVialCostsRequest request, CancellationToken ct)
    {
        await Mediator.Send(new SetVialCostsCommand(request.Items), ct);
        return NoContent();
    }
}

public record SetVialCostsRequest(List<VialCostDto> Items);
