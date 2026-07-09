using ChipoBackend.Application.Features.Expenses.Commands.CreateExpenseCategory;
using ChipoBackend.Application.Features.Expenses.Commands.ToggleExpenseCategoryStatus;
using ChipoBackend.Application.Features.Expenses.Commands.UpdateExpenseCategory;
using ChipoBackend.Application.Features.Expenses.Queries.GetExpenseCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/expense-categories")]
[Authorize(Roles = "SuperAdmin,Admin,Finance")]
public class ExpenseCategoriesController : BaseApiController
{
    // GET api/expense-categories  — all categories (active + inactive)
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetExpenseCategoriesQuery(IncludeInactive: true), ct);
        return Ok(result);
    }

    // GET api/expense-categories/active  — only active categories
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetExpenseCategoriesQuery(IncludeInactive: false), ct);
        return Ok(result);
    }

    // POST api/expense-categories
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCategoryCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetAll), new { }, new { id });
    }

    // PUT api/expense-categories/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseCategoryCommand command, CancellationToken ct)
    {
        var updated = command with { Id = id };
        await Mediator.Send(updated, ct);
        return NoContent();
    }

    // PATCH api/expense-categories/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken ct)
    {
        var isActive = await Mediator.Send(new ToggleExpenseCategoryStatusCommand(id), ct);
        return Ok(new { isActive });
    }
}
