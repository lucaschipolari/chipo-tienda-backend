using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChipoBackend.Application.Features.Promotions.Commands.CreateDiscount;
using ChipoBackend.Application.Features.Promotions.Commands.UpdateDiscount;
using ChipoBackend.Application.Features.Promotions.Commands.DeleteDiscount;
using ChipoBackend.Application.Features.Promotions.Commands.ToggleDiscount;
using ChipoBackend.Application.Features.Promotions.Queries.GetDiscounts;
using ChipoBackend.Application.Features.Promotions.Queries.GetDiscountById;

namespace ChipoBackend.API.Controllers;

public record CreateDiscountRequest(
    string Name,
    string Type,
    decimal Value,
    string AppliesTo,
    string? Description,
    string Currency,
    List<Guid>? TargetIds,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MinQuantity,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsStackable,
    int Priority,
    int? MaxUsage);

public record UpdateDiscountRequest(
    string Name,
    string Type,
    decimal Value,
    string AppliesTo,
    string? Description,
    string Currency,
    List<Guid>? TargetIds,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MinQuantity,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsStackable,
    int Priority,
    int? MaxUsage);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscountsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] string? appliesTo = null,
        [FromQuery] bool? isActive = null) =>
        Ok(await mediator.Send(new GetDiscountsQuery(page, pageSize, search, type, appliesTo, isActive)));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await mediator.Send(new GetDiscountByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRequest req)
    {
        var id = await mediator.Send(new CreateDiscountCommand(
            req.Name,
            req.Type,
            req.Value,
            req.AppliesTo,
            req.Description,
            req.Currency,
            req.TargetIds,
            req.MinOrderAmount,
            req.MaxDiscountAmount,
            req.MinQuantity,
            req.StartsAt,
            req.EndsAt,
            req.IsStackable,
            req.Priority,
            req.MaxUsage));

        return Created($"/api/discounts/{id}", new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountRequest body)
    {
        await mediator.Send(new UpdateDiscountCommand(
            id,
            body.Name,
            body.Type,
            body.Value,
            body.AppliesTo,
            body.Description,
            body.Currency,
            body.TargetIds,
            body.MinOrderAmount,
            body.MaxDiscountAmount,
            body.MinQuantity,
            body.StartsAt,
            body.EndsAt,
            body.IsStackable,
            body.Priority,
            body.MaxUsage));

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await mediator.Send(new DeleteDiscountCommand(id));
        return NoContent();
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        await mediator.Send(new ToggleDiscountCommand(id));
        return NoContent();
    }
}
