using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Commands.CreateExpense;

public record CreateExpenseCommand(
    Guid CategoryId,
    DateTime Date,
    decimal Amount,
    string Description,
    string? Observations,
    string? ReceiptUrl,
    string Currency = "ARS") : IRequest<Guid>;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
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

public class CreateExpenseCommandHandler(
    IExpenseRepository expenseRepository,
    IExpenseCategoryRepository categoryRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateExpenseCommand, Guid>
{
    public async Task<Guid> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException("ExpenseCategory", request.CategoryId);

        if (!category.IsActive)
            throw new ConflictException("La categoría seleccionada está inactiva.");

        var userId = currentUserService.UserId
            ?? throw new ForbiddenException("Usuario no autenticado.");

        var money = Money.Of(request.Amount, request.Currency);
        var expense = Expense.Create(request.CategoryId, request.Date, money, request.Description, request.Observations, request.ReceiptUrl, userId);

        await expenseRepository.AddAsync(expense, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return expense.Id;
    }
}
