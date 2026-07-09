using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Commands.ToggleExpenseCategoryStatus;

public record ToggleExpenseCategoryStatusCommand(Guid Id) : IRequest<bool>;

public class ToggleExpenseCategoryStatusCommandHandler(
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ToggleExpenseCategoryStatusCommand, bool>
{
    public async Task<bool> Handle(ToggleExpenseCategoryStatusCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("ExpenseCategory", request.Id);

        if (category.IsActive)
            category.Deactivate();
        else
            category.Activate();

        await unitOfWork.SaveChangesAsync(ct);
        return category.IsActive;
    }
}
