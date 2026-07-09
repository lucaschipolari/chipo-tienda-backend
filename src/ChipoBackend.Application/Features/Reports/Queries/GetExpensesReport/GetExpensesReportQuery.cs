using ChipoBackend.Application.Features.Reports.DTOs;
using ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Reports.Queries.GetExpensesReport;

public record GetExpensesReportQuery(
    DateTime From,
    DateTime To,
    Guid? CategoryId = null,
    string? Status = null
) : IRequest<ExpensesReportDto>;

public class GetExpensesReportQueryHandler(
    IExpenseRepository expenseRepository
) : IRequestHandler<GetExpensesReportQuery, ExpensesReportDto>
{
    public async Task<ExpensesReportDto> Handle(GetExpensesReportQuery request, CancellationToken ct)
    {
        ExpenseStatus? statusFilter = request.Status is not null
            ? Enum.TryParse<ExpenseStatus>(request.Status, ignoreCase: true, out var parsed)
                ? parsed
                : null
            : null;

        var (expenses, _) = await expenseRepository.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            categoryId: request.CategoryId,
            status: statusFilter,
            from: request.From,
            to: request.To,
            ct: ct);

        var rows = expenses.Select(e => new ExpensesReportRowDto(
            Date: e.Date,
            Category: e.Category?.Name ?? "—",
            Description: e.Description,
            Amount: e.Amount.Amount,
            Currency: e.Amount.Currency,
            Status: e.Status.ToString()
        )).ToList();

        var totalAmount = expenses.Sum(e => e.Amount.Amount);
        var currency = expenses.FirstOrDefault()?.Amount.Currency ?? "ARS";

        return new ExpensesReportDto(
            TotalCount: rows.Count,
            TotalAmount: totalAmount,
            Currency: currency,
            Rows: rows
        );
    }
}
