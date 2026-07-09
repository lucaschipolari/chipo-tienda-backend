using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetPromotions;

public record GetPromotionsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Type,
    bool? IsActive) : IRequest<PagedResult<PromotionListItemDto>>;

public class GetPromotionsQueryHandler(
    IPromotionRepository promotionRepository) : IRequestHandler<GetPromotionsQuery, PagedResult<PromotionListItemDto>>
{
    public async Task<PagedResult<PromotionListItemDto>> Handle(GetPromotionsQuery request, CancellationToken cancellationToken)
    {
        PromotionType? promotionType = request.Type is not null && Enum.TryParse<PromotionType>(request.Type, true, out var parsedType)
            ? parsedType
            : null;

        var (items, total) = await promotionRepository.GetPagedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            search: request.Search,
            type: promotionType,
            isActive: request.IsActive,
            ct: cancellationToken);

        var dtos = items.Select(p => new PromotionListItemDto(
            Id: p.Id,
            Name: p.Name,
            Description: p.Description,
            Type: p.Type.ToString(),
            Badge: p.Badge,
            DiscountType: p.DiscountType.ToString(),
            DiscountValue: p.DiscountValue,
            Currency: p.Currency,
            IsActive: p.IsActive,
            IsStackable: p.IsStackable,
            Priority: p.Priority,
            StartsAt: p.StartsAt,
            EndsAt: p.EndsAt,
            CreatedAt: p.CreatedAt)).ToList();

        return PagedResult<PromotionListItemDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
