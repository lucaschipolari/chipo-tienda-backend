using ChipoBackend.Application.Features.Inventory.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Inventory.Queries.GetLowStock;

public record GetLowStockQuery() : IRequest<List<LowStockItemDto>>;

public class GetLowStockQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetLowStockQuery, List<LowStockItemDto>>
{
    public async Task<List<LowStockItemDto>> Handle(GetLowStockQuery request, CancellationToken ct)
    {
        // Cargar todos los productos publicados con sus variantes
        var (products, _) = await productRepository.GetPagedAsync(
            1, 1000, status: Domain.Entities.Catalog.ProductStatus.Published, ct: ct);

        var lowStock = new List<LowStockItemDto>();

        foreach (var product in products)
        {
            foreach (var variant in product.Variants.Where(v => v.IsActive && v.IsBelowMinStock))
            {
                lowStock.Add(new LowStockItemDto(
                    ProductId: product.Id,
                    ProductName: product.Name,
                    ProductSlug: product.Slug,
                    VariantId: variant.Id,
                    VariantSku: variant.Sku,
                    Attributes: variant.Attributes,
                    StockQuantity: variant.StockQuantity,
                    MinStockThreshold: variant.MinStockThreshold,
                    Deficit: variant.MinStockThreshold - variant.StockQuantity
                ));
            }
        }

        return lowStock.OrderByDescending(x => x.Deficit).ToList();
    }
}
