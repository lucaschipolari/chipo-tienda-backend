using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Promotions.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Queries.GetDiscountById;

public record GetDiscountByIdQuery(Guid Id) : IRequest<DiscountDetailDto>;

public class GetDiscountByIdQueryHandler(
    IDiscountRepository discountRepository) : IRequestHandler<GetDiscountByIdQuery, DiscountDetailDto>
{
    public async Task<DiscountDetailDto> Handle(GetDiscountByIdQuery request, CancellationToken cancellationToken)
    {
        var discount = await discountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (discount is null || discount.IsDeleted)
            throw new NotFoundException("Discount", request.Id);

        return new DiscountDetailDto(
            Id: discount.Id,
            Name: discount.Name,
            Description: discount.Description,
            Type: discount.Type.ToString(),
            AppliesTo: discount.AppliesTo.ToString(),
            Value: discount.Value,
            Currency: discount.Currency,
            TargetIds: discount.TargetIds,
            MinOrderAmount: discount.MinOrderAmount?.Amount,
            MaxDiscountAmount: discount.MaxDiscountAmount?.Amount,
            MinQuantity: discount.MinQuantity,
            IsActive: discount.IsActive,
            IsStackable: discount.IsStackable,
            Priority: discount.Priority,
            MaxUsage: discount.MaxUsage,
            StartsAt: discount.StartsAt,
            EndsAt: discount.EndsAt,
            UsageCount: discount.UsageCount,
            CreatedAt: discount.CreatedAt,
            UpdatedAt: discount.UpdatedAt);
    }
}
