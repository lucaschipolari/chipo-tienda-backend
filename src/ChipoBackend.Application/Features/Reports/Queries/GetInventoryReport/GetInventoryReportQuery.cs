using ChipoBackend.Application.Features.Reports.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Reports.Queries.GetInventoryReport;

public record GetInventoryReportQuery(
    Guid? CategoryId = null,
    string? Status = null  // "OutOfStock" | "LowStock" | "Normal"
) : IRequest<InventoryReportDto>;

public class GetInventoryReportQueryHandler(
    IProductRepository productRepository
) : IRequestHandler<GetInventoryReportQuery, InventoryReportDto>
{
    public async Task<InventoryReportDto> Handle(GetInventoryReportQuery request, CancellationToken ct)
    {
        // Load all products (with variants) for the given category filter
        var (products, _) = await productRepository.GetPagedAsync(
            page: 1,
            pageSize: int.MaxValue,
            categoryId: request.CategoryId,
            ct: ct);

        // Load each product with its variants for stock data
        var lines = new List<InventoryReportRowDto>();

        foreach (var product in products)
        {
            var withVariants = await productRepository.GetWithVariantsAsync(product.Id, ct);
            if (withVariants is null) continue;

            foreach (var variant in withVariants.Variants)
            {
                var stockStatus = variant.StockQuantity == 0
                    ? "OutOfStock"
                    : variant.IsBelowMinStock
                        ? "Critical"
                        : "OK";

                if (!string.IsNullOrWhiteSpace(request.Status) && stockStatus != request.Status)
                    continue;

                var unitPrice = variant.Price?.Amount ?? withVariants.BasePrice.Amount;
                var unitCost = 0m; // Cost not tracked on variant; use 0 as placeholder
                var stockValue = unitPrice * variant.StockQuantity;

                var variantName = variant.Attributes.Count > 0
                    ? string.Join(" / ", variant.Attributes.Select(kv => $"{kv.Key}: {kv.Value}"))
                    : null;

                lines.Add(new InventoryReportRowDto(
                    ProductId: withVariants.Id,
                    ProductName: withVariants.Name,
                    VariantName: variantName,
                    Sku: variant.Sku,
                    Category: withVariants.Category?.Name ?? "—",
                    CurrentStock: variant.StockQuantity,
                    MinStock: variant.MinStockThreshold,
                    UnitCost: unitCost,
                    UnitPrice: unitPrice,
                    TotalValue: stockValue,
                    Status: stockStatus
                ));
            }
        }

        return new InventoryReportDto(
            Rows: lines,
            TotalProducts: lines.Count,
            OutOfStock: lines.Count(l => l.Status == "OutOfStock"),
            Critical: lines.Count(l => l.Status == "Critical"),
            TotalValue: lines.Sum(l => l.TotalValue)
        );
    }
}
