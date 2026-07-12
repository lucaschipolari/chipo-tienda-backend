using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Products.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Producto '{request.Id}' no encontrado.");

        return new ProductDto(
            Id: product.Id,
            CategoryId: product.CategoryId,
            CategoryName: product.Category?.Name,
            Name: product.Name,
            Slug: product.Slug,
            Description: product.Description,
            Sku: product.Sku,
            BasePrice: product.BasePrice.Amount,
            CompareAtPrice: product.CompareAtPrice?.Amount,
            Currency: product.BasePrice.Currency,
            Status: product.Status.ToString(),
            IsFeatured: product.IsFeatured,
            Tags: product.Tags,
            Olfactory: new OlfactoryProfileDto(
                TopNotes: product.TopNotes,
                HeartNotes: product.HeartNotes,
                BaseNotes: product.BaseNotes,
                Intensity: product.Intensity,
                Longevity: product.Longevity,
                Seasons: product.Seasons,
                Occasions: product.Occasions),
            Variants: product.Variants.Select(v => new ProductVariantDto(
                Id: v.Id,
                ProductId: v.ProductId,
                Sku: v.Sku,
                Barcode: v.Barcode,
                Attributes: v.Attributes,
                Price: v.Price is { Amount: > 0 } ? v.Price.Amount : (decimal?)null,
                CompareAtPrice: v.CompareAtPrice is { Amount: > 0 } ? v.CompareAtPrice.Amount : (decimal?)null,
                Cost: v.Cost is { Amount: > 0 } ? v.Cost.Amount : (decimal?)null,
                Currency: v.Price?.Currency ?? product.BasePrice.Currency,
                StockQuantity: v.StockQuantity,
                MinStockThreshold: v.MinStockThreshold,
                IsActive: v.IsActive,
                IsBelowMinStock: v.IsBelowMinStock,
                CreatedAt: v.CreatedAt,
                UpdatedAt: v.UpdatedAt
            )).ToList(),
            Images: product.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto(
                Id: i.Id,
                Url: i.Url,
                AltText: i.AltText,
                DisplayOrder: i.DisplayOrder
            )).ToList(),
            TotalStock: product.Variants.Sum(v => v.StockQuantity),
            CreatedAt: product.CreatedAt,
            UpdatedAt: product.UpdatedAt
        );
    }
}
