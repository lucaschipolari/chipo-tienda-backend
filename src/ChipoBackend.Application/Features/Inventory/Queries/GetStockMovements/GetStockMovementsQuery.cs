using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Inventory.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Inventory.Queries.GetStockMovements;

public record GetStockMovementsQuery(
    Guid? ProductId = null,
    Guid? VariantId = null,
    int Page = 1,
    int PageSize = 50
) : IRequest<PagedResult<StockMovementDto>>;

public class GetStockMovementsQueryHandler(
    IStockMovementRepository stockMovementRepository,
    IProductRepository productRepository
) : IRequestHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>>
{
    public async Task<PagedResult<StockMovementDto>> Handle(GetStockMovementsQuery request, CancellationToken ct)
    {
        var (movements, total) = await stockMovementRepository.GetPagedAsync(
            request.ProductId, request.VariantId, request.Page, request.PageSize, ct);

        // Cache de productos para enriquecer los DTOs sin N+1
        var productIds = movements.Select(m => m.ProductId).Distinct().ToList();
        var productCache = new Dictionary<Guid, (string Name, Dictionary<Guid, (string Sku, Dictionary<string, string> Attrs)> Variants)>();

        foreach (var productId in productIds)
        {
            var product = await productRepository.GetWithVariantsAsync(productId, ct);
            if (product != null)
            {
                var variantMap = product.Variants.ToDictionary(
                    v => v.Id,
                    v => (v.Sku, v.Attributes));
                productCache[productId] = (product.Name, variantMap);
            }
        }

        var dtos = movements.Select(m =>
        {
            productCache.TryGetValue(m.ProductId, out var pInfo);
            string variantSku = "—";
            Dictionary<string, string> variantAttrs = [];
            if (pInfo.Variants != null && pInfo.Variants.TryGetValue(m.VariantId, out var vInfo))
            {
                variantSku = vInfo.Sku;
                variantAttrs = vInfo.Attrs;
            }

            return new StockMovementDto(
                Id: m.Id,
                ProductId: m.ProductId,
                ProductName: pInfo.Name ?? "Desconocido",
                VariantId: m.VariantId,
                VariantSku: variantSku,
                VariantAttributes: variantAttrs,
                MovementType: m.MovementType.ToString(),
                Quantity: m.Quantity,
                StockBefore: m.StockBefore,
                StockAfter: m.StockAfter,
                Reason: m.Reason,
                ReferenceType: m.ReferenceType,
                ReferenceId: m.ReferenceId,
                CreatedByUserId: m.CreatedByUserId,
                CreatedAt: m.CreatedAt
            );
        }).ToList();

        return PagedResult<StockMovementDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
