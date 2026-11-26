using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.PricingCalculator;

/// <summary>
/// Panel de la calculadora: ajustes + una fila por producto (frascos y decants; sin la categoría Empaque).
/// <paramref name="Group"/>: "arabes" | "disenador" | "nicho" | "decants" (null = todos).
/// </summary>
public record GetPricingCalculatorQuery(string? Group = null, string? Search = null) : IRequest<PricingCalculatorOverviewDto>;

public class GetPricingCalculatorQueryHandler(
    IProductRepository products,
    IPricingCalculatorEntryRepository entries,
    IAppSettingRepository settingsRepository
) : IRequestHandler<GetPricingCalculatorQuery, PricingCalculatorOverviewDto>
{
    public async Task<PricingCalculatorOverviewDto> Handle(GetPricingCalculatorQuery request, CancellationToken ct)
    {
        var settings = await PricingSettings.LoadAsync(settingsRepository, ct);
        var all = await products.GetAllForPricingAsync(ct);
        var entryByProduct = await entries.GetAllByProductAsync(ct);

        var group = request.Group?.Trim().ToLowerInvariant();
        var search = request.Search?.Trim();

        var items = all
            .Where(p => !IsPackaging(p.Category?.Slug))
            .Where(p => string.IsNullOrEmpty(group) || group == "all" || PricingCalculatorBuilder.GroupFor(p) == group)
            .Where(p => string.IsNullOrEmpty(search)
                        || p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Select(p => PricingCalculatorBuilder.Build(p, entryByProduct.GetValueOrDefault(p.Id), settings))
            .ToList();

        return new PricingCalculatorOverviewDto(settings, items);
    }

    /// <summary>Los frasquitos y bolsas (categoría Empaque) son insumos, no perfumes a precio de venta.</summary>
    private static bool IsPackaging(string? slug) =>
        slug is not null && slug.Contains("empaque", StringComparison.OrdinalIgnoreCase);
}
