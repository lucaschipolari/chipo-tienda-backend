using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChipoBackend.Application.Features.Promotions.Commands.CreatePromotion;
using ChipoBackend.Application.Features.Promotions.Commands.UpdatePromotion;
using ChipoBackend.Application.Features.Promotions.Commands.TogglePromotion;
using ChipoBackend.Application.Features.Promotions.Commands.DuplicatePromotion;
using ChipoBackend.Application.Features.Promotions.Queries.GetPromotions;
using ChipoBackend.Application.Features.Promotions.Queries.GetPromotionsDashboard;
using ChipoBackend.Application.Features.Promotions.Queries.GetPromotionById;
using ChipoBackend.Application.Features.Promotions.Queries.CalculatePromotions;

namespace ChipoBackend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PromotionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] bool? isActive = null) =>
        Ok(await mediator.Send(new GetPromotionsQuery(page, pageSize, search, type, isActive)));

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard() =>
        Ok(await mediator.Send(new GetPromotionsDashboardQuery()));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        Ok(await mediator.Send(new GetPromotionByIdQuery(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionCommand command)
    {
        var id = await mediator.Send(command);
        return Created($"/api/promotions/{id}", new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePromotionCommand command)
    {
        await mediator.Send(command with { Id = id });
        return NoContent();
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        await mediator.Send(new TogglePromotionCommand(id));
        return NoContent();
    }

    [HttpPost("{id:guid}/duplicate")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var newId = await mediator.Send(new DuplicatePromotionCommand(id));
        return Created($"/api/promotions/{newId}", new { id = newId });
    }

    [AllowAnonymous]
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] CalculatePromotionsQuery query) =>
        Ok(await mediator.Send(query));
}
