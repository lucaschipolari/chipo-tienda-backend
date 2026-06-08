using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Exceptions;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Promotions;

public class Coupon : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public CouponType Type { get; private set; }
    public decimal Value { get; private set; }
    public Money? MinOrderAmount { get; private set; }
    public Money? MaxDiscountAmount { get; private set; }
    public int? UsageLimit { get; private set; }
    public int? UsageLimitPerUser { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<CouponUsage> _usages = [];
    public IReadOnlyCollection<CouponUsage> Usages => _usages.AsReadOnly();

    public bool IsCurrentlyValid => IsActive
        && (StartsAt == null || StartsAt <= DateTime.UtcNow)
        && (EndsAt == null || EndsAt >= DateTime.UtcNow)
        && (UsageLimit == null || UsedCount < UsageLimit);

    private Coupon() { }

    public static Coupon Create(string code, CouponType type, decimal value)
    {
        return new Coupon
        {
            Code = code.ToUpperInvariant().Trim(),
            Type = type,
            Value = value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
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
        var discount = Type == CouponType.Percentage
            ? orderTotal * (Value / 100m)
            : Type == CouponType.FixedAmount
                ? Money.Of(Value, orderTotal.Currency)
                : orderTotal; // FreeShipping handled separately

        if (MaxDiscountAmount != null && discount.Amount > MaxDiscountAmount.Amount)
            discount = MaxDiscountAmount;

        return discount;
    }

    public void RecordUsage(Guid customerId, Guid orderId)
    {
        _usages.Add(CouponUsage.Create(Id, customerId, orderId));
        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Toggle() { IsActive = !IsActive; UpdatedAt = DateTime.UtcNow; }
}

public enum CouponType { Percentage, FixedAmount, FreeShipping }
