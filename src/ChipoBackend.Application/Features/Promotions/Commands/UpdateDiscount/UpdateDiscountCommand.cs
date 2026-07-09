using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Commands.UpdateDiscount;

public record UpdateDiscountCommand(
    Guid Id,
    string Name,
    string Type,
    decimal Value,
    string AppliesTo,
    string? Description,
    string Currency,
    List<Guid>? TargetIds,
    decimal? MinOrderAmount,
    decimal? MaxDiscountAmount,
    int? MinQuantity,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool IsStackable,
    int Priority,
    int? MaxUsage) : IRequest<Unit>;

public class UpdateDiscountCommandValidator : AbstractValidator<UpdateDiscountCommand>
{
    public UpdateDiscountCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Value)
            .GreaterThan(0);

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<DiscountType>(t, true, out _))
            .WithMessage("Type must be a valid DiscountType (Percentage or FixedAmount).");

        RuleFor(x => x.AppliesTo)
            .NotEmpty()
            .Must(a => Enum.TryParse<DiscountAppliesTo>(a, true, out _))
            .WithMessage("AppliesTo must be a valid DiscountAppliesTo (Product, Category, Order, Cart, or Customer).");
    }
}

public class UpdateDiscountCommandHandler(
    IDiscountRepository discountRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateDiscountCommand, Unit>
{
    public async Task<Unit> Handle(UpdateDiscountCommand request, CancellationToken cancellationToken)
    {
        var discount = await discountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Discount", request.Id);

        var type = Enum.Parse<DiscountType>(request.Type, true);
        var appliesTo = Enum.Parse<DiscountAppliesTo>(request.AppliesTo, true);

        var minOrderAmount = request.MinOrderAmount.HasValue
            ? Money.Of(request.MinOrderAmount.Value, request.Currency)
            : null;

        var maxDiscountAmount = request.MaxDiscountAmount.HasValue
            ? Money.Of(request.MaxDiscountAmount.Value, request.Currency)
            : null;

        discount.Update(
            name: request.Name,
            type: type,
            value: request.Value,
            appliesTo: appliesTo,
            description: request.Description,
            currency: request.Currency,
            targetIds: request.TargetIds,
            minOrderAmount: minOrderAmount,
            maxDiscountAmount: maxDiscountAmount,
            minQuantity: request.MinQuantity,
            startsAt: request.StartsAt,
            endsAt: request.EndsAt,
            isStackable: request.IsStackable,
            priority: request.Priority,
            maxUsage: request.MaxUsage,
            updatedBy: currentUserService.UserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
