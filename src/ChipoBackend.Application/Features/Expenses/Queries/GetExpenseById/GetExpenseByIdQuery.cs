using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Expenses.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Queries.GetExpenseById;

public record GetExpenseByIdQuery(Guid Id) : IRequest<ExpenseDto>;

public class GetExpenseByIdQueryHandler(IExpenseRepository expenseRepository)
    : IRequestHandler<GetExpenseByIdQuery, ExpenseDto>
{
    public async Task<ExpenseDto> Handle(GetExpenseByIdQuery request, CancellationToken ct)
    {
        var expense = await expenseRepository.GetWithCategoryAsync(request.Id, ct)
            ?? throw new NotFoundException("Expense", request.Id);

        return new ExpenseDto(
            expense.Id,
            expense.CategoryId,
            expense.Category?.Name ?? string.Empty,
            expense.Category?.Color ?? "#6B7280",
            expense.Date,
            expense.Amount.Amount,
            expense.Amount.Currency,
            expense.Description,
            expense.Observations,
            expense.ReceiptUrl,
            expense.Status.ToString(),
            expense.CreatedByUserId,
            expense.CreatedAt,
            expense.UpdatedAt);
    }
}
