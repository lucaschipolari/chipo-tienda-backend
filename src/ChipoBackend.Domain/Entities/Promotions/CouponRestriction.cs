using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Promotions;

public enum RestrictionType { Product, Category, Customer }

public class CouponRestriction : BaseEntity
{
    public Guid CouponId { get; private set; }
    public RestrictionType Type { get; private set; }
    public Guid EntityId { get; private set; }

    private CouponRestriction() { }

    public static CouponRestriction Create(Guid couponId, RestrictionType type, Guid entityId)
        => new() { CouponId = couponId, Type = type, EntityId = entityId };
}
