using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetPromotionById;

public record GetPromotionByIdQuery(Guid Id) : IRequest<PromotionDetailDto>;

public class GetPromotionByIdQueryHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<GetPromotionByIdQuery, PromotionDetailDto>
{
    public async Task<PromotionDetailDto> Handle(GetPromotionByIdQuery request, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetWithRelationsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Promotion", request.Id);

        return new PromotionDetailDto(
            Id: promotion.Id,
            Name: promotion.Name,
            Description: promotion.Description,
            Type: promotion.Type.ToString(),
            Badge: promotion.Badge,
            DiscountType: promotion.DiscountType.ToString(),
            DiscountValue: promotion.DiscountValue,
            Currency: promotion.Currency,
            IsActive: promotion.IsActive,
            IsStackable: promotion.IsStackable,
            Priority: promotion.Priority,
            StartsAt: promotion.StartsAt,
            EndsAt: promotion.EndsAt,
            ActiveFrom: promotion.ActiveFrom?.ToString("HH:mm"),
            ActiveUntil: promotion.ActiveUntil?.ToString("HH:mm"),
            BuyQuantity: promotion.BuyQuantity,
            GetQuantity: promotion.GetQuantity,
            MinOrderAmount: promotion.MinOrderAmount,
            ComboPrice: promotion.ComboPrice,
            ProductIds: promotion.Products.Select(p => p.ProductId).ToList(),
            CategoryIds: promotion.Categories.Select(c => c.CategoryId).ToList(),
            CreatedAt: promotion.CreatedAt,
            UpdatedAt: promotion.UpdatedAt);
    }
}
