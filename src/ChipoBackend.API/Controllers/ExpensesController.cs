using ChipoBackend.Application.Features.Expenses.Commands.ChangeExpenseStatus;
using ChipoBackend.Application.Features.Expenses.Commands.CreateExpense;
using ChipoBackend.Application.Features.Expenses.Commands.DeleteExpense;
using ChipoBackend.Application.Features.Expenses.Commands.UpdateExpense;
using ChipoBackend.Application.Features.Expenses.Queries.GetExpenseDashboard;
using ChipoBackend.Application.Features.Expenses.Queries.GetExpenseById;
using ChipoBackend.Application.Features.Expenses.Queries.GetExpenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/expenses")]
[Authorize(Roles = "SuperAdmin,Admin,Finance,Supervisor")]
public class ExpensesController : BaseApiController
{
    // GET api/expenses
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetExpensesQuery(page, pageSize, categoryId, status, from, to, search), ct);
        return Ok(result);
    }

    // GET api/expenses/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetExpenseDashboardQuery(from, to), ct);
        return Ok(result);
    }

    // GET api/expenses/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetExpenseByIdQuery(id), ct);
        return Ok(result);
    }

    // POST api/expenses
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // PUT api/expenses/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseCommand command, CancellationToken ct)
    {
        var updated = command with { Id = id };
        await Mediator.Send(updated, ct);
        return NoContent();
    }

    // PATCH api/expenses/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeExpenseStatusRequest body, CancellationToken ct)
    {
        await Mediator.Send(new ChangeExpenseStatusCommand(id, body.NewStatus), ct);
        return NoContent();
    }

    // DELETE api/expenses/{id} — borrado físico, solo administradores
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Mediator.Send(new DeleteExpenseCommand(id), ct);
        return NoContent();
    }
}

public record ChangeExpenseStatusRequest(string NewStatus);
