using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Expenses;

public enum ExpenseStatus { Pending, Paid, Cancelled }

public class Expense : AuditableEntity
{
    public Guid CategoryId { get; private set; }
    public ExpenseCategory? Category { get; private set; }
    public DateTime Date { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public string Description { get; private set; } = string.Empty;
    public string? Observations { get; private set; }
    public string? ReceiptUrl { get; private set; }
    public ExpenseStatus Status { get; private set; } = ExpenseStatus.Pending;

    private Expense() { }

    public static Expense Create(
        Guid categoryId,
        DateTime date,
        Money amount,
        string description,
        string? observations,
        string? receiptUrl,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("La descripción del gasto es requerida.", nameof(description));

        return new Expense
        {
            CategoryId = categoryId,
            Date = date.Date,
            Amount = amount,
            Description = description.Trim(),
            Observations = observations?.Trim(),
            ReceiptUrl = receiptUrl?.Trim(),
            Status = ExpenseStatus.Pending,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        Guid categoryId,
        DateTime date,
        Money amount,
        string description,
        string? observations,
        string? receiptUrl)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("La descripción del gasto es requerida.", nameof(description));

        CategoryId = categoryId;
        Date = date.Date;
        Amount = amount;
        Description = description.Trim();
        Observations = observations?.Trim();
        ReceiptUrl = receiptUrl?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPaid()
    {
        if (Status == ExpenseStatus.Cancelled)
            throw new InvalidOperationException("No se puede marcar como pagado un gasto cancelado.");

        Status = ExpenseStatus.Paid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == ExpenseStatus.Paid)
            throw new InvalidOperationException("No se puede cancelar un gasto ya pagado.");

        Status = ExpenseStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
