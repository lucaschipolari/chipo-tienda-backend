using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Commands.UpdateExpenseCategory;

public record UpdateExpenseCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Color) : IRequest;

public class UpdateExpenseCategoryCommandValidator : AbstractValidator<UpdateExpenseCategoryCommand>
{
    public UpdateExpenseCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es requerido.")
            .MaximumLength(100).WithMessage("El nombre no puede superar 100 caracteres.");
    }
}

public class UpdateExpenseCategoryCommandHandler(
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateExpenseCategoryCommand>
{
    public async Task Handle(UpdateExpenseCategoryCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("ExpenseCategory", request.Id);

        var existing = await categoryRepository.GetByNameAsync(request.Name.Trim(), ct);
        if (existing != null && existing.Id != request.Id)
            throw new ConflictException($"Ya existe una categoría con el nombre '{request.Name}'.");

        category.Update(request.Name, request.Description, request.Color);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
