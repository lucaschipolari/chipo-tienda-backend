using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetDiscounts;

public record GetDiscountsQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Type,
    string? AppliesTo,
    bool? IsActive) : IRequest<PagedResult<DiscountListItemDto>>;

public class GetDiscountsQueryHandler(
    IDiscountRepository discountRepository) : IRequestHandler<GetDiscountsQuery, PagedResult<DiscountListItemDto>>
{
    public async Task<PagedResult<DiscountListItemDto>> Handle(GetDiscountsQuery request, CancellationToken cancellationToken)
    {
        DiscountType? type = request.Type is not null && Enum.TryParse<DiscountType>(request.Type, true, out var parsedType)
            ? parsedType
            : null;

        DiscountAppliesTo? appliesTo = request.AppliesTo is not null && Enum.TryParse<DiscountAppliesTo>(request.AppliesTo, true, out var parsedAppliesTo)
            ? parsedAppliesTo
            : null;

        var (items, total) = await discountRepository.GetPagedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            search: request.Search,
            type: type,
            appliesTo: appliesTo,
            isActive: request.IsActive,
            ct: cancellationToken);

        var dtos = items.Select(d => new DiscountListItemDto(
            Id: d.Id,
            Name: d.Name,
            Description: d.Description,
            Type: d.Type.ToString(),
            AppliesTo: d.AppliesTo.ToString(),
            Value: d.Value,
            Currency: d.Currency,
            IsActive: d.IsActive,
            IsStackable: d.IsStackable,
            Priority: d.Priority,
            StartsAt: d.StartsAt,
            EndsAt: d.EndsAt,
            UsageCount: d.UsageCount,
            MaxUsage: d.MaxUsage,
            CreatedAt: d.CreatedAt)).ToList();

        return PagedResult<DiscountListItemDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
