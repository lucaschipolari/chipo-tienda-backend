using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetCoupons;

public record GetCouponsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Type,
    bool? IsActive) : IRequest<PagedResult<CouponListItemDto>>;

public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, PagedResult<CouponListItemDto>>
{
    private readonly ICouponRepository _couponRepository;

    public GetCouponsQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<PagedResult<CouponListItemDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        CouponType? parsedType = null;
        if (!string.IsNullOrWhiteSpace(request.Type) && Enum.TryParse<CouponType>(request.Type, out var t))
            parsedType = t;

        var (items, total) = await _couponRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Search,
            parsedType,
            request.IsActive,
            cancellationToken);

        var dtos = items.Select(c => new CouponListItemDto(
            c.Id,
            c.Code,
            c.Name,
            c.Description,
            c.Type.ToString(),
            c.Value,
            c.Currency,
            c.UsedCount,
            c.UsageLimit,
            c.IsActive,
            c.IsStackable,
            c.StartsAt,
            c.EndsAt,
            c.CreatedAt)).ToList();

        return PagedResult<CouponListItemDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
