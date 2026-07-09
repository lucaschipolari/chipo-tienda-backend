using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Commands.CreateExpenseCategory;

public record CreateExpenseCategoryCommand(
    string Name,
    string? Description,
    string? Color) : IRequest<Guid>;

public class CreateExpenseCategoryCommandValidator : AbstractValidator<CreateExpenseCategoryCommand>
{
    public CreateExpenseCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
    }
}

public class CreateExpenseCategoryCommandHandler(
    IExpenseCategoryRepository categoryRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateExpenseCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateExpenseCategoryCommand request, CancellationToken ct)
    {
        var existing = await categoryRepository.GetByNameAsync(request.Name.Trim(), ct);
        if (existing != null)
            throw new ConflictException($"Ya existe una categoría con el nombre '{request.Name}'.");

        var userId = currentUserService.UserId
            ?? throw new ForbiddenException("Usuario no autenticado.");

        var category = ExpenseCategory.Create(request.Name, request.Description, request.Color, userId);
        await categoryRepository.AddAsync(category, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return category.Id;
    }
}
