using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using AppValidationException = ChipoBackend.Application.Common.Exceptions.ValidationException;
using ConflictException = ChipoBackend.Application.Common.Exceptions.ConflictException;

namespace ChipoBackend.Application.Features.Promotions.Commands.CreateCoupon;

public record CouponRestrictionInput(string Type, Guid EntityId);

public record CreateCouponCommand(
    string Code,
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
    List<CouponRestrictionInput>? Restrictions) : IRequest<Guid>;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    private static readonly string[] ValidTypes = ["Percentage", "FixedAmount", "FreeShipping"];

    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código del cupón es requerido.")
            .MaximumLength(50).WithMessage("El código no puede superar 50 caracteres.");

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

public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, Guid>
{
    private readonly ICouponRepository _couponRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCouponCommandHandler(
        ICouponRepository couponRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _couponRepository.CodeExistsAsync(request.Code, ct: cancellationToken);
        if (codeExists)
            throw new ConflictException($"Ya existe un cupón con el código '{request.Code}'.");

        if (!Enum.TryParse<CouponType>(request.Type, out var couponType))
            throw new AppValidationException(new Dictionary<string, string[]> { ["Type"] = [$"Tipo de cupón inválido: {request.Type}"] });

        var minOrderAmount = request.MinOrderAmount.HasValue
            ? Money.Of(request.MinOrderAmount.Value, request.Currency)
            : (Money?)null;

        var maxDiscountAmount = request.MaxDiscountAmount.HasValue
            ? Money.Of(request.MaxDiscountAmount.Value, request.Currency)
            : (Money?)null;

        var coupon = Coupon.Create(
            code: request.Code,
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
            isStackable: request.IsStackable,
            createdBy: _currentUserService.UserId);

        if (request.Restrictions != null)
        {
            foreach (var r in request.Restrictions)
            {
                if (!Enum.TryParse<RestrictionType>(r.Type, out var restrictionType))
                    throw new AppValidationException(new Dictionary<string, string[]> { ["Restrictions"] = [$"Tipo de restricción inválido: {r.Type}"] });

                coupon.AddRestriction(restrictionType, r.EntityId);
            }
        }

        await _couponRepository.AddAsync(coupon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return coupon.Id;
    }
}
