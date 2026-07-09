using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Commands.DuplicatePromotion;

public record DuplicatePromotionCommand(Guid Id) : IRequest<Guid>;

public class DuplicatePromotionCommandHandler(
    IPromotionRepository promotionRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IRequestHandler<DuplicatePromotionCommand, Guid>
{
    public async Task<Guid> Handle(DuplicatePromotionCommand request, CancellationToken cancellationToken)
    {
        var original = await promotionRepository.GetWithRelationsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Promotion", request.Id);

        var createdBy = currentUserService.UserId;

        var duplicate = Promotion.Create(
            name: original.Name + " (copia)",
            type: original.Type,
            discountType: original.DiscountType,
            discountValue: original.DiscountValue,
            startsAt: original.StartsAt,
            endsAt: original.EndsAt,
            description: original.Description,
            badge: original.Badge,
            currency: original.Currency,
            isStackable: original.IsStackable,
            priority: original.Priority,
            activeFrom: original.ActiveFrom,
            activeUntil: original.ActiveUntil,
            buyQuantity: original.BuyQuantity,
            getQuantity: original.GetQuantity,
            minOrderAmount: original.MinOrderAmount,
            comboPrice: original.ComboPrice,
            createdBy: createdBy);

        await promotionRepository.AddAsync(duplicate, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var productIds = original.Products.Select(p => p.ProductId).ToList();
        var categoryIds = original.Categories.Select(c => c.CategoryId).ToList();

        if (productIds.Count > 0)
            duplicate.SetProducts(productIds);

        if (categoryIds.Count > 0)
            duplicate.SetCategories(categoryIds);

        if (productIds.Count > 0 || categoryIds.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return duplicate.Id;
    }
}
