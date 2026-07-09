namespace ChipoBackend.Application.Features.Expenses.DTOs;

public record ExpenseCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string Color,
    bool IsActive,
    int ExpenseCount);

public record ExpenseDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    DateTime Date,
    decimal Amount,
    string Currency,
    string Description,
    string? Observations,
    string? ReceiptUrl,
    string Status,
    Guid? CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ExpenseListItemDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string CategoryColor,
    DateTime Date,
    decimal Amount,
    string Currency,
    string Description,
    string Status,
    DateTime CreatedAt);

public record ExpenseDashboardDto(
    decimal TodayTotal,
    decimal WeekTotal,
    decimal MonthTotal,
    decimal YearTotal,
    List<ExpenseByCategoryDto> ByCategory,
    List<ExpenseTrendDto> MonthlyTrend);

public record ExpenseByCategoryDto(
    Guid CategoryId,
    string CategoryName,
    string Color,
    decimal Total,
    int Count,
    decimal Percentage);

public record ExpenseTrendDto(string Month, decimal Total);

public record CreateExpenseCategoryRequest(
    string Name,
    string? Description,
    string? Color);

public record UpdateExpenseCategoryRequest(
    Guid Id,
    string Name,
    string? Description,
    string? Color);

public record CreateExpenseRequest(
    Guid CategoryId,
    DateTime Date,
    decimal Amount,
    string Currency,
    string Description,
    string? Observations,
    string? ReceiptUrl);

public record UpdateExpenseRequest(
    Guid Id,
    Guid CategoryId,
    DateTime Date,
    decimal Amount,
    string Currency,
    string Description,
    string? Observations,
    string? ReceiptUrl);
