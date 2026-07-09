using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.ValidateCoupon;

public record ValidateCouponQuery(
    string Code,
    decimal OrderTotal,
    string Currency,
    Guid? CustomerId) : IRequest<ValidateCouponResultDto>;

public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, ValidateCouponResultDto>
{
    private readonly ICouponRepository _couponRepository;

    public ValidateCouponQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<ValidateCouponResultDto> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetWithUsagesByCodeAsync(request.Code, cancellationToken);

        if (coupon is null || coupon.IsDeleted)
            return Invalid("Cupón no encontrado.", request.Currency);

        if (!coupon.IsCurrentlyValid)
            return Invalid("Cupón inválido o expirado.", request.Currency);

        if (coupon.MinOrderAmount != null && request.OrderTotal < coupon.MinOrderAmount.Amount)
            return Invalid(
                $"Monto mínimo de {coupon.MinOrderAmount.Amount} {coupon.MinOrderAmount.Currency} no alcanzado.",
                request.Currency);

        if (request.CustomerId.HasValue && coupon.UsageLimitPerUser.HasValue)
        {
            var usagesByCustomer = coupon.Usages?.Count(u => u.CustomerId == request.CustomerId) ?? 0;
            if (usagesByCustomer >= coupon.UsageLimitPerUser.Value)
                return Invalid("Has alcanzado el límite de uso de este cupón.", request.Currency);
        }

        var orderMoney = Money.Of(request.OrderTotal, request.Currency);
        var discountAmount = coupon.CalculateDiscount(orderMoney);

        return new ValidateCouponResultDto(
            IsValid: true,
            ErrorMessage: null,
            CouponType: coupon.Type.ToString(),
            DiscountValue: coupon.Value,
            DiscountAmount: discountAmount.Amount,
            Currency: request.Currency);
    }

    private static ValidateCouponResultDto Invalid(string errorMessage, string currency) =>
        new(IsValid: false, ErrorMessage: errorMessage, CouponType: null,
            DiscountValue: 0, DiscountAmount: 0, Currency: currency);
}
