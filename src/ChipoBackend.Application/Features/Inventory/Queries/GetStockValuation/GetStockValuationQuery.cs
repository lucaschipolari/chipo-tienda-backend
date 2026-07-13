using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Inventory.Queries.GetStockValuation;

public record StockValuationDto(
    decimal TotalCostValue,     // capital invertido = Σ (costo × stock)
    decimal TotalRetailValue,   // valor a precio de venta = Σ (precio × stock)
    int TotalUnits,             // unidades totales en stock
    int VariantsWithoutCost     // variantes con stock pero sin costo cargado (dato incompleto)
);

public record GetStockValuationQuery : IRequest<StockValuationDto>;

public class GetStockValuationQueryHandler(IProductRepository productRepository)
    : IRequestHandler<GetStockValuationQuery, StockValuationDto>
{
    public async Task<StockValuationDto> Handle(GetStockValuationQuery request, CancellationToken ct)
    {
        var (products, _) = await productRepository.GetPagedAsync(1, 100000, ct: ct);

        decimal costValue = 0, retailValue = 0;
        int units = 0, withoutCost = 0;

        foreach (var p in products)
        {
            // Los accesorios de empaque no cuentan como capital de mercadería.
            if (string.Equals(p.Category?.Name, "Empaque", StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var v in p.Variants.Where(v => v.IsActive && v.StockQuantity > 0))
            {
                units += v.StockQuantity;
                var price = v.Price?.Amount ?? p.BasePrice.Amount;
                retailValue += price * v.StockQuantity;

                if (v.Cost is { } cost)
                    costValue += cost.Amount * v.StockQuantity;
                else
                    withoutCost++;
            }
        }

        return new StockValuationDto(costValue, retailValue, units, withoutCost);
    }
}
