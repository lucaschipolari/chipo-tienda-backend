using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetCouponById;

public record GetCouponByIdQuery(Guid Id) : IRequest<CouponDetailDto>;

public class GetCouponByIdQueryHandler : IRequestHandler<GetCouponByIdQuery, CouponDetailDto>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponByIdQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<CouponDetailDto> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetWithRestrictionsAsync(request.Id, cancellationToken);

        if (coupon is null || coupon.IsDeleted)
            throw new NotFoundException(nameof(Coupon), request.Id);

        var restrictions = coupon.Restrictions
            .Select(r => new CouponRestrictionDto(r.Type.ToString(), r.EntityId))
            .ToList();

        return new CouponDetailDto(
            coupon.Id,
            coupon.Code,
            coupon.Name,
            coupon.Description,
            coupon.Type.ToString(),
            coupon.Value,
            coupon.Currency,
            coupon.MinOrderAmount?.Amount,
            coupon.MaxDiscountAmount?.Amount,
            coupon.UsedCount,
            coupon.UsageLimit,
            coupon.UsageLimitPerUser,
            coupon.IsActive,
            coupon.IsStackable,
            coupon.StartsAt,
            coupon.EndsAt,
            restrictions,
            coupon.CreatedAt,
            coupon.UpdatedAt);
    }
}
