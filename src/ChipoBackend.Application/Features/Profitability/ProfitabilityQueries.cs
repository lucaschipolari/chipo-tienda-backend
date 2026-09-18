using ChipoBackend.Application.Features.Settings;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Profitability;

// ── Builder compartido ─────────────────────────────────────────────────────────
/// <summary>Arma las filas de rentabilidad (una por variante) aplicando las reglas de costo y cálculo.</summary>
public class ProfitabilityRowBuilder(
    IProductRepository productRepository,
    IProductCostHistoryRepository costHistoryRepository,
    IAppSettingRepository appSettings)
{
    public async Task<(List<ProfitabilityRowDto> Rows, ProfitabilitySettingsDto Settings)> BuildAsync(CancellationToken ct)
    {
        var settings = ProfitabilitySettings.Parse((await appSettings.GetAsync(ProfitabilitySettings.Key, ct))?.Value);
        var vialCosts = VialCostSettings.Parse((await appSettings.GetAsync(VialCostSettings.Key, ct))?.Value);

        var products = await productRepository.GetAllWithVariantsAndCategoryAsync(ct);
        var allHistory = await costHistoryRepository.GetAllOrderedAsync(ct);
        var historyByVariant = allHistory
            .GroupBy(h => h.VariantId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ProductCostHistory>)g.OrderByDescending(h => h.RecordedAt).ToList());

        var rows = new List<ProfitabilityRowDto>();
        foreach (var product in products)
        {
            foreach (var variant in product.Variants)
            {
                var vHistory = historyByVariant.TryGetValue(variant.Id, out var h) ? h : [];
                var (last, prev) = ProfitabilityCostResolver.Resolve(product, variant, vHistory, vialCosts);
                var price = ProfitabilityCostResolver.SalePrice(product, variant);

                var r = ProfitabilityCalculator.Analyze(price, last, prev, product.TargetMarginPct, settings);

                rows.Add(new ProfitabilityRowDto(
                    ProductId: product.Id,
                    VariantId: variant.Id,
                    ProductName: product.Name,
                    CategoryId: product.CategoryId,
                    Brand: product.Category?.Name,
                    Sku: variant.Sku,
                    VariantLabel: ProfitabilityCostResolver.VariantLabel(variant),
                    IsDecant: product.IsDecant,
                    Currency: product.BasePrice?.Currency ?? "ARS",
                    SalePrice: r.SalePrice,
                    LastCost: r.LastCost,
                    PreviousCost: r.PreviousCost,
                    CostChangeAbs: r.CostChangeAbs,
                    CostChangePct: r.CostChangePct,
                    Profit: r.Profit,
                    MarginPct: r.MarginPct,
                    TargetMarginPct: r.TargetMarginPct,
                    TargetIsCustom: r.TargetIsCustom,
                    SuggestedPrice: r.SuggestedPrice,
                    PriceDifference: r.PriceDifference,
                    Status: r.Status
                ));
            }
        }
        return (rows, settings);
    }
}

// ── Tabla general ──────────────────────────────────────────────────────────────
public record GetProfitabilityQuery(
    Guid? CategoryId = null,
    string? Status = null,
    string? Search = null,
    bool? CostIncreased = null,
    bool? BelowSuggested = null,
    decimal? MinMargin = null,
    decimal? MaxMargin = null,
    string? SortBy = null,      // margin | lastCost | costChange | priceDiff
    string? SortDir = null      // asc | desc
) : IRequest<List<ProfitabilityRowDto>>;

public class GetProfitabilityQueryHandler(ProfitabilityRowBuilder builder)
    : IRequestHandler<GetProfitabilityQuery, List<ProfitabilityRowDto>>
{
    public async Task<List<ProfitabilityRowDto>> Handle(GetProfitabilityQuery request, CancellationToken ct)
    {
        var (rows, _) = await builder.BuildAsync(ct);
        IEnumerable<ProfitabilityRowDto> q = rows;

        if (request.CategoryId is { } cat)
            q = q.Where(r => r.CategoryId == cat);
        if (!string.IsNullOrWhiteSpace(request.Status))
            q = q.Where(r => string.Equals(r.Status, request.Status, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim();
            q = q.Where(r =>
                r.ProductName.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                r.Sku.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                (r.Brand?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (request.CostIncreased == true)
            q = q.Where(r => r.CostChangeAbs is > 0);
        if (request.BelowSuggested == true)
            q = q.Where(r => r.PriceDifference is > 0);
        if (request.MinMargin is { } min)
            q = q.Where(r => r.MarginPct is not null && r.MarginPct >= min);
        if (request.MaxMargin is { } max)
            q = q.Where(r => r.MarginPct is not null && r.MarginPct <= max);

        var desc = !string.Equals(request.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        q = request.SortBy?.ToLowerInvariant() switch
        {
            "lastcost"   => Order(q, r => r.LastCost, desc),
            "costchange" => Order(q, r => r.CostChangePct, desc),
            "pricediff"  => Order(q, r => r.PriceDifference, desc),
            "margin"     => Order(q, r => r.MarginPct, desc),
            _            => q.OrderBy(r => r.ProductName)
        };

        return q.ToList();
    }

    // Ordena poniendo los null siempre al final, sin importar la dirección.
    private static IEnumerable<ProfitabilityRowDto> Order(
        IEnumerable<ProfitabilityRowDto> q, Func<ProfitabilityRowDto, decimal?> key, bool desc)
    {
        var withValue = q.Where(r => key(r) is not null);
        var nulls = q.Where(r => key(r) is null);
        var ordered = desc ? withValue.OrderByDescending(r => key(r)) : withValue.OrderBy(r => key(r));
        return ordered.Concat(nulls);
    }
}

// ── Resumen (cards) ────────────────────────────────────────────────────────────
public record GetProfitabilitySummaryQuery : IRequest<ProfitabilitySummaryDto>;

public class GetProfitabilitySummaryQueryHandler(ProfitabilityRowBuilder builder)
    : IRequestHandler<GetProfitabilitySummaryQuery, ProfitabilitySummaryDto>
{
    public async Task<ProfitabilitySummaryDto> Handle(GetProfitabilitySummaryQuery request, CancellationToken ct)
    {
        var (rows, _) = await builder.BuildAsync(ct);
        return new ProfitabilitySummaryDto(
            TotalAnalyzed: rows.Count,
            Optimo: rows.Count(r => r.Status == ProfitabilityStatus.Optimo),
            Bajo: rows.Count(r => r.Status == ProfitabilityStatus.Bajo),
            Critico: rows.Count(r => r.Status == ProfitabilityStatus.Critico),
            SinDatos: rows.Count(r => r.Status == ProfitabilityStatus.SinDatos),
            CostIncreased: rows.Count(r => r.CostChangeAbs is > 0),
            NeedsPriceReview: rows.Count(r =>
                r.Status is ProfitabilityStatus.Bajo or ProfitabilityStatus.Critico || r.PriceDifference is > 0)
        );
    }
}

// ── Detalle de un producto ───────────────────────────────────────────────────────
public record GetProductProfitabilityQuery(Guid ProductId) : IRequest<ProductProfitabilityDetailDto?>;

public class GetProductProfitabilityQueryHandler(
    IProductRepository productRepository,
    IProductCostHistoryRepository costHistoryRepository,
    IAppSettingRepository appSettings
) : IRequestHandler<GetProductProfitabilityQuery, ProductProfitabilityDetailDto?>
{
    public async Task<ProductProfitabilityDetailDto?> Handle(GetProductProfitabilityQuery request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct);
        if (product is null) return null;

        var settings = ProfitabilitySettings.Parse((await appSettings.GetAsync(ProfitabilitySettings.Key, ct))?.Value);
        var vialCosts = VialCostSettings.Parse((await appSettings.GetAsync(VialCostSettings.Key, ct))?.Value);

        // Variante principal para el análisis (la de menor DisplayOrder / primera activa).
        var variant = product.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault(v => v.IsActive)
                      ?? product.Variants.OrderBy(v => v.DisplayOrder).FirstOrDefault();
        if (variant is null) return null;

        var vHistory = await costHistoryRepository.GetByVariantAsync(variant.Id, ct);
        var (last, prev) = ProfitabilityCostResolver.Resolve(product, variant, vHistory, vialCosts);
        var price = ProfitabilityCostResolver.SalePrice(product, variant);
        var r = ProfitabilityCalculator.Analyze(price, last, prev, product.TargetMarginPct, settings);

        var analysis = new ProfitabilityRowDto(
            ProductId: product.Id, VariantId: variant.Id, ProductName: product.Name,
            CategoryId: product.CategoryId, Brand: product.Category?.Name, Sku: variant.Sku,
            VariantLabel: ProfitabilityCostResolver.VariantLabel(variant), IsDecant: product.IsDecant,
            Currency: product.BasePrice?.Currency ?? "ARS",
            SalePrice: r.SalePrice, LastCost: r.LastCost, PreviousCost: r.PreviousCost,
            CostChangeAbs: r.CostChangeAbs, CostChangePct: r.CostChangePct, Profit: r.Profit,
            MarginPct: r.MarginPct, TargetMarginPct: r.TargetMarginPct, TargetIsCustom: r.TargetIsCustom,
            SuggestedPrice: r.SuggestedPrice, PriceDifference: r.PriceDifference, Status: r.Status);

        // Historial cronológico (viejo → nuevo) con variación contra el registro anterior.
        var chrono = vHistory.OrderBy(h => h.RecordedAt).ToList();
        var history = new List<CostHistoryEntryDto>();
        for (int i = 0; i < chrono.Count; i++)
        {
            var cur = chrono[i];
            decimal? prevAmt = i > 0 ? chrono[i - 1].UnitCost.Amount : null;
            var (abs, pct) = ProfitabilityCalculator.CostChange(prevAmt, cur.UnitCost.Amount);
            history.Add(new CostHistoryEntryDto(
                Id: cur.Id, UnitCost: cur.UnitCost.Amount, Currency: cur.UnitCost.Currency,
                Source: cur.Source.ToString(), PurchaseOrderId: cur.PurchaseOrderId,
                RecordedAt: cur.RecordedAt, ChangeAbs: abs, ChangePct: pct));
        }
        history.Reverse(); // más reciente primero para la UI

        return new ProductProfitabilityDetailDto(analysis, history);
    }
}
