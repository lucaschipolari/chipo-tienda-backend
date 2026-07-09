using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Promotions.Commands.CreateCoupon;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using AppValidationException = ChipoBackend.Application.Common.Exceptions.ValidationException;

namespace ChipoBackend.Application.Features.Promotions.Commands.UpdateCoupon;

public record UpdateCouponCommand(
    Guid Id,
    string Name,
    string Type,
    decimal Value,
    string Currency,
    string? Description,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int? UsageLimitPerUser,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsStackable,
    List<CouponRestrictionInput>? Restrictions) : IRequest<Unit>;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    private static readonly string[] ValidTypes = ["Percentage", "FixedAmount", "FreeShipping"];

    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El Id del cupón es requerido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cupón es requerido.");

        RuleFor(x => x.Value)
            .GreaterThan(0).When(x => x.Type != "FreeShipping")
            .WithMessage("El valor del cupón debe ser mayor a 0.");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).When(x => x.Type == "FreeShipping")
            .WithMessage("El valor no puede ser negativo.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => ValidTypes.Contains(t))
            .WithMessage($"El tipo debe ser uno de: {string.Join(", ", ValidTypes)}.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("La moneda es requerida.");
    }
}

public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, Unit>
{
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCouponCommandHandler(ICouponRepository couponRepository, IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Coupon), request.Id);

        if (coupon.IsDeleted)
            throw new NotFoundException(nameof(Coupon), request.Id);

        if (!Enum.TryParse<CouponType>(request.Type, out var couponType))
            throw new AppValidationException(new Dictionary<string, string[]> { ["Type"] = [$"Tipo de cupón inválido: {request.Type}"] });

        var minOrderAmount = request.MinOrderAmount.HasValue
            ? Money.Of(request.MinOrderAmount.Value, request.Currency)
            : (Money?)null;

        var maxDiscountAmount = request.MaxDiscountAmount.HasValue
            ? Money.Of(request.MaxDiscountAmount.Value, request.Currency)
            : (Money?)null;

        coupon.Update(
            name: request.Name,
            type: couponType,
            value: request.Value,
            currency: request.Currency,
            description: request.Description,
            minOrderAmount: minOrderAmount,
            maxDiscountAmount: maxDiscountAmount,
            usageLimit: request.UsageLimit,
            usageLimitPerUser: request.UsageLimitPerUser,
            startsAt: request.StartsAt,
            endsAt: request.EndsAt,
            isStackable: request.IsStackable);

        if (request.Restrictions != null)
        {
            coupon.ClearRestrictions();
            foreach (var r in request.Restrictions)
            {
                if (!Enum.TryParse<RestrictionType>(r.Type, out var restrictionType))
                    throw new AppValidationException(new Dictionary<string, string[]> { ["Restrictions"] = [$"Tipo de restricción inválido: {r.Type}"] });

                coupon.AddRestriction(restrictionType, r.EntityId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
