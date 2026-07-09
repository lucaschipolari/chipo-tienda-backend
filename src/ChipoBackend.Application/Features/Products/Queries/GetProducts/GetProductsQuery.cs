using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Products.DTOs;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    string? Search = null,
    string? Status = null
) : IRequest<PagedResult<ProductListItemDto>>;

public class GetProductsQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    public async Task<PagedResult<ProductListItemDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        ProductStatus? statusFilter = request.Status?.ToLowerInvariant() switch
        {
            "draft"        => ProductStatus.Draft,
            "published"    => ProductStatus.Published,
            "discontinued" => ProductStatus.Discontinued,
            _ => null
        };

        var (products, total) = await productRepository.GetPagedAsync(
            request.Page, request.PageSize,
            request.CategoryId, request.Search, statusFilter, ct);

        var dtos = products.Select(p =>
        {
            var activeVariants = p.Variants.Where(v => v.IsActive).ToList();
            // Quick-add directo solo cuando hay una única variante activa para elegir
            var defaultVariant = activeVariants.Count == 1 ? activeVariants[0] : null;
            return new ProductListItemDto(
                Id: p.Id,
                CategoryId: p.CategoryId,
                CategoryName: p.Category?.Name,
                Name: p.Name,
                Slug: p.Slug,
                Sku: p.Sku,
                BasePrice: p.BasePrice.Amount,
                CompareAtPrice: p.CompareAtPrice?.Amount,
                Currency: p.BasePrice.Currency,
                Status: p.Status.ToString(),
                IsFeatured: p.IsFeatured,
                TotalStock: p.Variants.Sum(v => v.StockQuantity),
                VariantCount: p.Variants.Count,
                DefaultVariantId: defaultVariant?.Id,
                DefaultVariantStock: defaultVariant?.StockQuantity ?? 0,
                MainImageUrl: p.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.Url,
                Description: p.Description,
                Notes: p.TopNotes.Concat(p.HeartNotes).Concat(p.BaseNotes).ToList(),
                CreatedAt: p.CreatedAt,
                UpdatedAt: p.UpdatedAt
            );
        }).ToList();

        return PagedResult<ProductListItemDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
