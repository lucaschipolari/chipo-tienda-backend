using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Promotions;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DateTime UsedAt { get; private set; }

    private CouponUsage() { }

    public static CouponUsage Create(Guid couponId, Guid? customerId, Guid orderId, decimal discountAmount)
        => new() { CouponId = couponId, CustomerId = customerId, OrderId = orderId, DiscountAmount = discountAmount, UsedAt = DateTime.UtcNow };
}
