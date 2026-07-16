using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Commands.DeleteExpense;

/// <summary>
/// Elimina físicamente un gasto. Reservado a administradores: permite corregir
/// cargas erróneas sin importar el estado del gasto (incluso Pagado).
/// </summary>
public record DeleteExpenseCommand(Guid Id) : IRequest;

public class DeleteExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteExpenseCommand>
{
    public async Task Handle(DeleteExpenseCommand request, CancellationToken ct)
    {
        var expense = await expenseRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Expense", request.Id);

        expenseRepository.Remove(expense);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
