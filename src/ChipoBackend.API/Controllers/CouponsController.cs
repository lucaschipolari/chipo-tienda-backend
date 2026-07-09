using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChipoBackend.Application.Features.Promotions.Commands.CreateCoupon;
using ChipoBackend.Application.Features.Promotions.Commands.UpdateCoupon;
using ChipoBackend.Application.Features.Promotions.Commands.ToggleCoupon;
using ChipoBackend.Application.Features.Promotions.Commands.DeleteCoupon;
using ChipoBackend.Application.Features.Promotions.Queries.GetCoupons;
using ChipoBackend.Application.Features.Promotions.Queries.GetCouponById;
using ChipoBackend.Application.Features.Promotions.Queries.GetCouponUsages;
using ChipoBackend.Application.Features.Promotions.Queries.ValidateCoupon;

namespace ChipoBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CouponsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null) =>
        Ok(await mediator.Send(new GetCouponsQuery(page, pageSize, search, type, isActive)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await mediator.Send(new GetCouponByIdQuery(id)));

    [HttpGet("{id:guid}/usages")]
    public async Task<IActionResult> GetUsages(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await mediator.Send(new GetCouponUsagesQuery(id, page, pageSize)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCouponCommand command)
    {
        var newId = await mediator.Send(command);
        return Created($"/api/coupons/{newId}", new { id = newId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponCommand command)
    {
        await mediator.Send(command with { Id = id });
        return NoContent();
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        await mediator.Send(new ToggleCouponCommand(id));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteCouponCommand(id));
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponQuery query) =>
        Ok(await mediator.Send(query));
}
