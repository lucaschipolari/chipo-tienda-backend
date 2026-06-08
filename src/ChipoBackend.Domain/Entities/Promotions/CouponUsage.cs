using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Promotions;

public class CouponUsage : BaseEntity
{
    public Guid CouponId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid OrderId { get; private set; }
    public DateTime UsedAt { get; private set; }

    private CouponUsage() { }

    public static CouponUsage Create(Guid couponId, Guid customerId, Guid orderId)
    {
        return new CouponUsage { CouponId = couponId, CustomerId = customerId, OrderId = orderId, UsedAt = DateTime.UtcNow };
    }
}
