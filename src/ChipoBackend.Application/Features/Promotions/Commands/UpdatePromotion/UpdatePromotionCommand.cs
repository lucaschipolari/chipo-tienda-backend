using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Commands.UpdatePromotion;

public record UpdatePromotionCommand(
    Guid Id,
    string Name,
    string Type,
    string DiscountType,
    decimal DiscountValue,
    DateTime StartsAt,
    DateTime? EndsAt,
    string? Description,
    string? Badge,
    string Currency,
    bool IsStackable,
    int Priority,
    string? ActiveFrom,
    string? ActiveUntil,
    int? BuyQuantity,
    int? GetQuantity,
    decimal? MinOrderAmount,
    decimal? ComboPrice,
    List<Guid>? ProductIds,
    List<Guid>? CategoryIds) : IRequest<Unit>;

public class UpdatePromotionCommandValidator : AbstractValidator<UpdatePromotionCommand>
{
    public UpdatePromotionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

        RuleFor(x => x.StartsAt)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(t => Enum.TryParse<PromotionType>(t, true, out _))
            .WithMessage("Type must be a valid PromotionType.");

        When(x => x.Type == PromotionType.BuyXGetY.ToString(), () =>
        {
            RuleFor(x => x.BuyQuantity)
                .NotNull().WithMessage("BuyQuantity is required for BuyXGetY promotions.")
                .GreaterThan(0).WithMessage("BuyQuantity must be greater than 0.");

            RuleFor(x => x.GetQuantity)
                .NotNull().WithMessage("GetQuantity is required for BuyXGetY promotions.")
                .GreaterThan(0).WithMessage("GetQuantity must be greater than 0.");
        });

        When(x => x.Type == PromotionType.MinAmount.ToString(), () =>
        {
            RuleFor(x => x.MinOrderAmount)
                .NotNull().WithMessage("MinOrderAmount is required for MinAmount promotions.");
        });
    }
}

public class UpdatePromotionCommandHandler(
    IPromotionRepository promotionRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdatePromotionCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Promotion", request.Id);

        var promotionType = Enum.Parse<PromotionType>(request.Type, true);
        var discountType = Enum.Parse<DiscountType>(request.DiscountType, true);

        TimeOnly? activeFrom = null;
        if (!string.IsNullOrWhiteSpace(request.ActiveFrom) && TimeOnly.TryParse(request.ActiveFrom, out var parsedFrom))
            activeFrom = parsedFrom;

        TimeOnly? activeUntil = null;
        if (!string.IsNullOrWhiteSpace(request.ActiveUntil) && TimeOnly.TryParse(request.ActiveUntil, out var parsedUntil))
            activeUntil = parsedUntil;

        var updatedBy = currentUserService.UserId;

        promotion.Update(
            name: request.Name,
            type: promotionType,
            discountType: discountType,
            discountValue: request.DiscountValue,
            startsAt: request.StartsAt,
            endsAt: request.EndsAt,
            description: request.Description,
            badge: request.Badge,
            currency: request.Currency,
            isStackable: request.IsStackable,
            priority: request.Priority,
            activeFrom: activeFrom,
            activeUntil: activeUntil,
            buyQuantity: request.BuyQuantity,
            getQuantity: request.GetQuantity,
            minOrderAmount: request.MinOrderAmount,
            comboPrice: request.ComboPrice,
            updatedBy: updatedBy);

        if (request.ProductIds is not null)
            promotion.SetProducts(request.ProductIds);

        if (request.CategoryIds is not null)
            promotion.SetCategories(request.CategoryIds);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
