using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Commands.UpdateExpense;

public record UpdateExpenseCommand(
    Guid Id,
    Guid CategoryId,
    DateTime Date,
    decimal Amount,
    string Description,
    string? Observations,
    string? ReceiptUrl,
    string Currency = "ARS") : IRequest;

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es requerida.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha del gasto es requerida.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es requerida.")
            .MaximumLength(3).WithMessage("La moneda debe tener máximo 3 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es requerida.")
            .MaximumLength(500).WithMessage("La descripción no puede superar 500 caracteres.");
    }
}

public class UpdateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateExpenseCommand>
{
    public async Task Handle(UpdateExpenseCommand request, CancellationToken ct)
    {
        var expense = await expenseRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Expense", request.Id);

        if (expense.Status != ExpenseStatus.Pending)
            throw new ConflictException("Solo se pueden editar gastos en estado Pendiente.");

        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException("ExpenseCategory", request.CategoryId);

        if (!category.IsActive)
            throw new ConflictException("La categoría seleccionada está inactiva.");

        var money = Money.Of(request.Amount, request.Currency);
        expense.Update(request.CategoryId, request.Date, money, request.Description, request.Observations, request.ReceiptUrl);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
