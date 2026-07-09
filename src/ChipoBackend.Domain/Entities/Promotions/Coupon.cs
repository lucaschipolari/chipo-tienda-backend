using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Exceptions;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Promotions;

public enum CouponType { Percentage, FixedAmount, FreeShipping }

public class Coupon : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public CouponType Type { get; private set; }
    public decimal Value { get; private set; }
    public string Currency { get; private set; } = "ARS";
    public Money? MinOrderAmount { get; private set; }
    public Money? MaxDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int? UsageLimitPerUser { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsStackable { get; private set; } = false;
    public bool IsDeleted { get; private set; } = false;

    private readonly List<CouponUsage> _usages = [];
    private readonly List<CouponRestriction> _restrictions = [];
    public IReadOnlyCollection<CouponUsage> Usages => _usages.AsReadOnly();
    public IReadOnlyCollection<CouponRestriction> Restrictions => _restrictions.AsReadOnly();

    public bool IsCurrentlyValid => !IsDeleted && IsActive
        && (StartsAt == null || StartsAt <= DateTime.UtcNow)
        && (EndsAt == null || EndsAt >= DateTime.UtcNow)
        && (UsageLimit == null || UsedCount < UsageLimit);

    private Coupon() { }

    public static Coupon Create(string code, string name, CouponType type, decimal value,
        string currency = "ARS", string? description = null,
        Money? minOrderAmount = null, Money? maxDiscountAmount = null,
        int? usageLimit = null, int? usageLimitPerUser = null,
        DateTime? startsAt = null, DateTime? endsAt = null,
        bool isStackable = false, Guid? createdBy = null)
    {
        return new Coupon
        {
            Code = code.ToUpperInvariant().Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            Value = value,
            Currency = currency.ToUpperInvariant(),
            MinOrderAmount = minOrderAmount,
            MaxDiscountAmount = maxDiscountAmount,
            UsageLimit = usageLimit,
            UsageLimitPerUser = usageLimitPerUser,
            StartsAt = startsAt,
            EndsAt = endsAt,
            IsStackable = isStackable,
            CreatedByUserId = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Update(string name, CouponType type, decimal value, string currency,
        string? description, Money? minOrderAmount, Money? maxDiscountAmount,
        int? usageLimit, int? usageLimitPerUser,
        DateTime? startsAt, DateTime? endsAt, bool isStackable, Guid? updatedBy = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Type = type;
        Value = value;
        Currency = currency.ToUpperInvariant();
        MinOrderAmount = minOrderAmount;
        MaxDiscountAmount = maxDiscountAmount;
        UsageLimit = usageLimit;
        UsageLimitPerUser = usageLimitPerUser;
        StartsAt = startsAt;
        EndsAt = endsAt;
        IsStackable = isStackable;
        UpdatedByUserId = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ValidateForCustomer(Guid customerId, Money orderTotal)
    {
        if (!IsCurrentlyValid)
            throw new BusinessRuleException("InvalidCoupon", "El cupón no es válido o ha expirado.");
        if (MinOrderAmount != null && orderTotal.Amount < MinOrderAmount.Amount)
            throw new BusinessRuleException("OrderBelowMinimum", $"El pedido debe ser de al menos {MinOrderAmount} para usar este cupón.");
        if (UsageLimitPerUser != null)
        {
            var userUsages = _usages.Count(u => u.CustomerId == customerId);
            if (userUsages >= UsageLimitPerUser)
                throw new BusinessRuleException("CouponLimitReached", "Has alcanzado el límite de uso de este cupón.");
        }
    }

    public Money CalculateDiscount(Money orderTotal)
    {
        var discount = Type switch
        {
            CouponType.Percentage => orderTotal * (Value / 100m),
            CouponType.FixedAmount => Money.Of(Value, Currency),
            _ => Money.Zero(orderTotal.Currency),
        };
        if (MaxDiscountAmount != null && discount.Amount > MaxDiscountAmount.Amount)
            discount = MaxDiscountAmount;
        return discount;
    }

    public void RecordUsage(Guid? customerId, Guid orderId, decimal discountAmount)
    {
        _usages.Add(CouponUsage.Create(Id, customerId, orderId, discountAmount));
        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRestriction(RestrictionType type, Guid entityId)
    {
        _restrictions.Add(CouponRestriction.Create(Id, type, entityId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearRestrictions()
    {
        _restrictions.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Toggle() { IsActive = !IsActive; UpdatedAt = DateTime.UtcNow; }
    public void SoftDelete() { IsDeleted = true; IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
