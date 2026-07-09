using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Commands.ChangeExpenseStatus;

public record ChangeExpenseStatusCommand(Guid Id, string NewStatus) : IRequest;

public class ChangeExpenseStatusCommandValidator : AbstractValidator<ChangeExpenseStatusCommand>
{
    public ChangeExpenseStatusCommandValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("El nuevo estado es requerido.")
            .Must(s => s == "Paid" || s == "Cancelled")
            .WithMessage("El estado debe ser 'Paid' o 'Cancelled'.");
    }
}

public class ChangeExpenseStatusCommandHandler(
    IExpenseRepository expenseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangeExpenseStatusCommand>
{
    public async Task Handle(ChangeExpenseStatusCommand request, CancellationToken ct)
    {
        var expense = await expenseRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Expense", request.Id);

        if (expense.Status == ExpenseStatus.Cancelled)
            throw new ConflictException("No se puede cambiar el estado de un gasto cancelado.");

        switch (request.NewStatus)
        {
            case "Paid":
                expense.MarkPaid();
                break;
            case "Cancelled":
                expense.Cancel();
                break;
            default:
                throw new ConflictException($"Estado inválido: '{request.NewStatus}'.");
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
