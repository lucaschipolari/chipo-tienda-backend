using System.Text.RegularExpressions;
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
            Variants: product.Variants
                .OrderBy(v => v.DisplayOrder)
                .ThenBy(v => Ml(v.Attributes))
                .Select(v => new ProductVariantDto(
                Id: v.Id,
                ProductId: v.ProductId,
                Sku: v.Sku,
                Barcode: v.Barcode,
                Attributes: v.Attributes,
                Price: v.Price is { Amount: > 0 } ? v.Price.Amount : (decimal?)null,
                CompareAtPrice: v.CompareAtPrice is { Amount: > 0 } ? v.CompareAtPrice.Amount : (decimal?)null,
                Cost: v.Cost is { Amount: > 0 } ? v.Cost.Amount : (decimal?)null,
                Currency: v.Price?.Currency ?? product.BasePrice.Currency,
                // En decants el stock por tamaño se deriva del pool de ml (ml disponibles ÷ ml del tamaño)
                StockQuantity: product.IsDecant
                    ? (Ml(v.Attributes) > 0 ? product.StockMl / Ml(v.Attributes) : 0)
                    : v.StockQuantity,
                MinStockThreshold: v.MinStockThreshold,
                DisplayOrder: v.DisplayOrder,
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
            IsDecant: product.IsDecant,
            StockMl: product.StockMl,
            BottleCost: product.BottleCost,
            BottleMl: product.BottleMl,
            ReorderMl: product.ReorderMl,
            CostPerMl: product.CostPerMl,
            CreatedAt: product.CreatedAt,
            UpdatedAt: product.UpdatedAt
        );
    }

    private static int Ml(Dictionary<string, string> attributes)
    {
        if (attributes == null) return 0;
        foreach (var v in attributes.Values)
        {
            var m = Regex.Match(v ?? "", @"(\d+)\s*ml", RegexOptions.IgnoreCase);
            if (m.Success) return int.Parse(m.Groups[1].Value);
        }
        return 0;
    }
}
