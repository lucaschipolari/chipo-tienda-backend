using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Promotions;

public class Discount : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public DiscountType Type { get; private set; }
    public decimal Value { get; private set; }
    public DiscountAppliesTo AppliesTo { get; private set; }
    public List<Guid> TargetIds { get; private set; } = [];
    public Money? MinOrderAmount { get; private set; }
    public Money? MaxDiscountAmount { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int UsageCount { get; private set; }
    public int? MaxUsage { get; private set; }

    public bool IsCurrentlyValid => IsActive
        && (StartsAt == null || StartsAt <= DateTime.UtcNow)
        && (EndsAt == null || EndsAt >= DateTime.UtcNow)
        && (MaxUsage == null || UsageCount < MaxUsage);

    private Discount() { }

    public static Discount Create(string name, DiscountType type, decimal value, DiscountAppliesTo appliesTo)
    {
        return new Discount
        {
            Name = name,
            Type = type,
            Value = value,
            AppliesTo = appliesTo,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Toggle() { IsActive = !IsActive; UpdatedAt = DateTime.UtcNow; }
    public void IncrementUsage() { UsageCount++; UpdatedAt = DateTime.UtcNow; }

    public Money CalculateDiscount(Money orderTotal)
    {
        if (MinOrderAmount != null && orderTotal.Amount < MinOrderAmount.Amount)
            return Money.Zero(orderTotal.Currency);

        var discountAmount = Type == DiscountType.Percentage
            ? orderTotal * (Value / 100m)
            : Money.Of(Value, orderTotal.Currency);

        if (MaxDiscountAmount != null && discountAmount.Amount > MaxDiscountAmount.Amount)
            discountAmount = MaxDiscountAmount;

        return discountAmount;
    }
}

public enum DiscountType { Percentage, FixedAmount }
public enum DiscountAppliesTo { Product, Category, Order }
