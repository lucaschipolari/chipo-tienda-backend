using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Promotions;

public enum DiscountType { Percentage, FixedAmount }
public enum DiscountAppliesTo { Product, Category, Order, Cart, Customer }

public class Discount : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DiscountType Type { get; private set; }
    public decimal Value { get; private set; }
    public string Currency { get; private set; } = "ARS";
    public DiscountAppliesTo AppliesTo { get; private set; }
    public List<Guid> TargetIds { get; private set; } = [];
    public Money? MinOrderAmount { get; private set; }
    public Money? MaxDiscountAmount { get; private set; }
    public int? MinQuantity { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsStackable { get; private set; } = true;
    public bool IsDeleted { get; private set; } = false;
    public int Priority { get; private set; } = 0;
    public int UsageCount { get; private set; }
    public int? MaxUsage { get; private set; }

    public bool IsCurrentlyValid => !IsDeleted && IsActive
        && (StartsAt == null || StartsAt <= DateTime.UtcNow)
        && (EndsAt == null || EndsAt >= DateTime.UtcNow)
        && (MaxUsage == null || UsageCount < MaxUsage);

    private Discount() { }

    public static Discount Create(string name, DiscountType type, decimal value, DiscountAppliesTo appliesTo,
        string? description = null, string currency = "ARS", List<Guid>? targetIds = null,
        Money? minOrderAmount = null, Money? maxDiscountAmount = null, int? minQuantity = null,
        DateTime? startsAt = null, DateTime? endsAt = null, bool isStackable = true,
        int priority = 0, int? maxUsage = null, Guid? createdBy = null)
    {
        return new Discount
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            Value = value,
            Currency = currency.ToUpperInvariant(),
            AppliesTo = appliesTo,
            TargetIds = targetIds ?? [],
            MinOrderAmount = minOrderAmount,
            MaxDiscountAmount = maxDiscountAmount,
            MinQuantity = minQuantity,
            StartsAt = startsAt,
            EndsAt = endsAt,
            IsActive = true,
            IsStackable = isStackable,
            Priority = priority,
            MaxUsage = maxUsage,
            CreatedByUserId = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Update(string name, DiscountType type, decimal value, DiscountAppliesTo appliesTo,
        string? description, string currency, List<Guid>? targetIds,
        Money? minOrderAmount, Money? maxDiscountAmount, int? minQuantity,
        DateTime? startsAt, DateTime? endsAt, bool isStackable, int priority, int? maxUsage, Guid? updatedBy = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Type = type;
        Value = value;
        Currency = currency.ToUpperInvariant();
        AppliesTo = appliesTo;
        TargetIds = targetIds ?? [];
        MinOrderAmount = minOrderAmount;
        MaxDiscountAmount = maxDiscountAmount;
        MinQuantity = minQuantity;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsStackable = isStackable;
        Priority = priority;
        MaxUsage = maxUsage;
        UpdatedByUserId = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Toggle() { IsActive = !IsActive; UpdatedAt = DateTime.UtcNow; }
    public void SoftDelete() { IsDeleted = true; IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void IncrementUsage() { UsageCount++; UpdatedAt = DateTime.UtcNow; }

    public Money CalculateDiscount(Money orderTotal)
    {
        if (MinOrderAmount != null && orderTotal.Amount < MinOrderAmount.Amount)
            return Money.Zero(orderTotal.Currency);
        var discountAmount = Type == DiscountType.Percentage
            ? orderTotal * (Value / 100m)
            : Money.Of(Value, Currency);
        if (MaxDiscountAmount != null && discountAmount.Amount > MaxDiscountAmount.Amount)
            discountAmount = MaxDiscountAmount;
        return discountAmount;
    }
}
