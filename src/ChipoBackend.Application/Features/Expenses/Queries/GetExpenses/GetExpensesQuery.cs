using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Expenses.DTOs;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Expenses.Queries.GetExpenses;

public record GetExpensesQuery(
    int Page,
    int PageSize,
    Guid? CategoryId,
    string? Status,
    DateTime? From,
    DateTime? To,
    string? Search) : IRequest<PagedResult<ExpenseListItemDto>>;

public class GetExpensesQueryHandler(IExpenseRepository expenseRepository)
    : IRequestHandler<GetExpensesQuery, PagedResult<ExpenseListItemDto>>
{
    public async Task<PagedResult<ExpenseListItemDto>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        ExpenseStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ExpenseStatus>(request.Status, out var parsed))
            statusFilter = parsed;

        var (items, total) = await expenseRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.CategoryId,
            statusFilter,
            request.From,
            request.To,
            request.Search,
            ct);

        var dtos = items.Select(e => new ExpenseListItemDto(
            e.Id,
            e.CategoryId,
            e.Category?.Name ?? string.Empty,
            e.Category?.Color ?? "#6B7280",
            e.Date,
            e.Amount.Amount,
            e.Amount.Currency,
            e.Description,
            e.Status.ToString(),
            e.CreatedAt)).ToList();

        return PagedResult<ExpenseListItemDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
