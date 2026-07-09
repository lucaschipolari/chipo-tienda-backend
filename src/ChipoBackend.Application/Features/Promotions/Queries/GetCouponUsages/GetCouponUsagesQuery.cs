using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetCouponUsages;

public record GetCouponUsagesQuery(
    Guid CouponId,
    int Page,
    int PageSize) : IRequest<PagedResult<CouponUsageDto>>;

public class GetCouponUsagesQueryHandler : IRequestHandler<GetCouponUsagesQuery, PagedResult<CouponUsageDto>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponUsagesQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<PagedResult<CouponUsageDto>> Handle(GetCouponUsagesQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetWithUsagesByIdAsync(request.CouponId, cancellationToken);

        if (coupon is null || coupon.IsDeleted)
            throw new NotFoundException(nameof(Coupon), request.CouponId);

        var allUsages = coupon.Usages ?? [];
        var total = allUsages.Count;

        var pagedUsages = allUsages
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new CouponUsageDto(
                u.Id,
                u.CustomerId,
                u.OrderId,
                u.DiscountAmount,
                u.UsedAt))
            .ToList();

        return PagedResult<CouponUsageDto>.Create(pagedUsages, total, request.Page, request.PageSize);
    }
}
