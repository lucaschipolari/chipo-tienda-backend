using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Finance;

public class Expense : AuditableEntity
{
    public ExpenseCategory Category { get; private set; }
    public string Description { get; private set; } = null!;
    public Money Amount { get; private set; } = null!;
    public Guid? SupplierId { get; private set; }
    public string PaymentMethod { get; private set; } = null!;
    public string? ReceiptUrl { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public Guid? RegisteredByUserId { get; private set; }
    public string? Notes { get; private set; }

    private Expense() { }

    public static Expense Create(ExpenseCategory category, string description, Money amount, string paymentMethod, DateTime occurredAt, Guid? registeredByUserId = null)
    {
        return new Expense
        {
            Category = category,
            Description = description,
            Amount = amount,
            PaymentMethod = paymentMethod,
            OccurredAt = occurredAt,
            RegisteredByUserId = registeredByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(ExpenseCategory category, string description, Money amount, string paymentMethod, DateTime occurredAt, Guid? supplierId, string? receiptUrl, string? notes)
    {
        Category = category;
        Description = description;
        Amount = amount;
        PaymentMethod = paymentMethod;
        OccurredAt = occurredAt;
        SupplierId = supplierId;
        ReceiptUrl = receiptUrl;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum ExpenseCategory { Rent, Utilities, Salaries, Marketing, Supplies, Transport, Taxes, Other }
