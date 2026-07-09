using ChipoBackend.Application.Features.Expenses.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Queries.GetExpenseCategories;

public record GetExpenseCategoriesQuery(bool IncludeInactive = false) : IRequest<List<ExpenseCategoryDto>>;

public class GetExpenseCategoriesQueryHandler(
    IExpenseCategoryRepository categoryRepository,
    IExpenseRepository expenseRepository)
    : IRequestHandler<GetExpenseCategoriesQuery, List<ExpenseCategoryDto>>
{
    public async Task<List<ExpenseCategoryDto>> Handle(GetExpenseCategoriesQuery request, CancellationToken ct)
    {
        IReadOnlyList<Domain.Entities.Expenses.ExpenseCategory> categories;

        if (request.IncludeInactive)
        {
            var (items, _) = await categoryRepository.GetPagedAsync(1, int.MaxValue, null, null, ct);
            categories = items;
        }
        else
        {
            categories = await categoryRepository.GetActiveAsync(ct);
        }

        var dtos = new List<ExpenseCategoryDto>();
        foreach (var cat in categories)
        {
            var (expenses, expenseCount) = await expenseRepository.GetPagedAsync(
                1, 1, cat.Id, null, null, null, null, ct);

            dtos.Add(new ExpenseCategoryDto(
                cat.Id,
                cat.Name,
                cat.Description,
                cat.Color,
                cat.IsActive,
                expenseCount));
        }

        return dtos;
    }
}
