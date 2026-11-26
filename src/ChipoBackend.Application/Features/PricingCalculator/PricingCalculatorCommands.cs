using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.PricingCalculator;

// ── Ajustes ────────────────────────────────────────────────────────────────────
/// <summary>Actualiza la cotización, los gastos de compra, los márgenes por grupo y los extras de packaging. Null = no cambiar.</summary>
public record UpdatePricingSettingsCommand(
    decimal? UsdtRate = null,
    decimal? CommissionPercentage = null,
    decimal? CommissionFixedStandard = null,
    decimal? CommissionFixedCheap = null,
    decimal? ShippingCost = null,
    List<GroupMarginDto>? Margins = null,
    List<PackagingExtraDto>? DecantPackagingExtras = null
) : IRequest<PricingSettingsDto>;

public class UpdatePricingSettingsCommandValidator : AbstractValidator<UpdatePricingSettingsCommand>
{
    public UpdatePricingSettingsCommandValidator()
    {
        RuleFor(x => x.UsdtRate).GreaterThan(0).When(x => x.UsdtRate.HasValue)
            .WithMessage("La cotización USDT tiene que ser mayor a 0.");
        RuleFor(x => x.CommissionPercentage).GreaterThanOrEqualTo(0).When(x => x.CommissionPercentage.HasValue);
        RuleFor(x => x.CommissionFixedStandard).GreaterThanOrEqualTo(0).When(x => x.CommissionFixedStandard.HasValue);
        RuleFor(x => x.CommissionFixedCheap).GreaterThanOrEqualTo(0).When(x => x.CommissionFixedCheap.HasValue);
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0).When(x => x.ShippingCost.HasValue);
        RuleForEach(x => x.Margins).ChildRules(m =>
        {
            m.RuleFor(i => i.Group).Must(g => PricingSettings.Groups.Any(x => x.Group == g))
                .WithMessage("Grupo de margen desconocido.");
            m.RuleFor(i => i.MarginType).Must(t => PricingFormula.NormalizeMarginType(t) is not null)
                .WithMessage("El tipo de margen tiene que ser porcentaje o monto fijo.");
            m.RuleFor(i => i.MarginValue).GreaterThanOrEqualTo(0).WithMessage("El margen no puede ser negativo.");
        });
        RuleForEach(x => x.DecantPackagingExtras).ChildRules(e =>
        {
            e.RuleFor(i => i.Name).NotEmpty().WithMessage("Cada extra de packaging necesita un nombre.");
            e.RuleFor(i => i.Cost).GreaterThanOrEqualTo(0).WithMessage("El costo de un extra no puede ser negativo.");
        });
    }
}

public class UpdatePricingSettingsCommandHandler(IAppSettingRepository settings, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePricingSettingsCommand, PricingSettingsDto>
{
    public async Task<PricingSettingsDto> Handle(UpdatePricingSettingsCommand request, CancellationToken ct)
    {
        async Task Set(string key, decimal? value)
        {
            if (value.HasValue) await PricingSettings.SetAsync(settings, key, PricingSettings.Format(value.Value), ct);
        }

        await Set(PricingSettings.UsdtRateKey, request.UsdtRate);
        await Set(PricingSettings.CommissionPercentageKey, request.CommissionPercentage);
        await Set(PricingSettings.CommissionFixedStandardKey, request.CommissionFixedStandard);
        await Set(PricingSettings.CommissionFixedCheapKey, request.CommissionFixedCheap);
        await Set(PricingSettings.ShippingCostKey, request.ShippingCost);

        foreach (var m in request.Margins ?? [])
        {
            await PricingSettings.SetAsync(settings, PricingSettings.MarginTypeKey(m.Group), PricingFormula.NormalizeMarginType(m.MarginType)!, ct);
            await Set(PricingSettings.MarginValueKey(m.Group), m.MarginValue);
        }

        if (request.DecantPackagingExtras is not null)
        {
            var clean = request.DecantPackagingExtras.Select(e => new PackagingExtraDto(e.Name.Trim(), e.Cost));
            await PricingSettings.SetAsync(settings, PricingSettings.PackagingExtrasKey, PricingSettings.SerializeExtras(clean), ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return await PricingSettings.LoadAsync(settings, ct);
    }
}

// ── Datos de compra y margen de un producto ───────────────────────────────────
/// <summary>
/// Guarda los datos de un producto en la calculadora y sincroniza el costo de sus variantes con lo calculado.
/// En un decant solo cuenta el margen (el costo sale del costo del frasco cargado en el producto).
/// </summary>
public record UpdateProductPricingEntryCommand(
    Guid ProductId,
    decimal CostUsdOrigin,
    decimal PercentageCommission,
    decimal FixedCommission,
    decimal ShippingCost,
    string MarginType,
    decimal MarginValue
) : IRequest<ProductPricingItemDto>;

public class UpdateProductPricingEntryCommandValidator : AbstractValidator<UpdateProductPricingEntryCommand>
{
    public UpdateProductPricingEntryCommandValidator()
    {
        RuleFor(x => x.CostUsdOrigin).GreaterThanOrEqualTo(0).WithMessage("El costo en USD no puede ser negativo.");
        RuleFor(x => x.PercentageCommission).GreaterThanOrEqualTo(0);
        RuleFor(x => x.FixedCommission).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MarginType).Must(t => PricingFormula.NormalizeMarginType(t) is not null)
            .WithMessage("El tipo de margen tiene que ser porcentaje o monto fijo.");
        RuleFor(x => x.MarginValue).GreaterThanOrEqualTo(0).WithMessage("El margen no puede ser negativo.");
    }
}

public class UpdateProductPricingEntryCommandHandler(
    IProductRepository products,
    IPricingCalculatorEntryRepository entries,
    IAppSettingRepository settingsRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateProductPricingEntryCommand, ProductPricingItemDto>
{
    public async Task<ProductPricingItemDto> Handle(UpdateProductPricingEntryCommand request, CancellationToken ct)
    {
        var product = await products.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");
        var settings = await PricingSettings.LoadAsync(settingsRepository, ct);

        // Un decant no tiene costo en USD propio: se guardan ceros y solo vale el margen.
        var (usd, pct, fixedComm, shipping) = product.IsDecant
            ? (0m, 0m, 0m, 0m)
            : (request.CostUsdOrigin, request.PercentageCommission, request.FixedCommission, request.ShippingCost);

        var entry = await entries.GetByProductIdAsync(product.Id, ct);
        if (entry is null)
        {
            entry = PricingCalculatorEntry.Create(product.Id, usd, pct, fixedComm, shipping, request.MarginType, request.MarginValue);
            entries.Add(entry);
        }
        else
        {
            entry.Update(usd, pct, fixedComm, shipping, request.MarginType, request.MarginValue);
        }

        if (product.IsDecant)
        {
            foreach (var size in PricingCalculatorBuilder.PriceDecant(product, entry.MarginType, entry.MarginValue, settings))
                if (size.LiquidCost > 0)
                    PricingCalculatorBuilder.SyncVariantCost(product, size.Variant, size.Cost, unitOfWork, currentUser.UserId);
        }
        else if (PricingCalculatorBuilder.MainVariant(product) is { } main)
        {
            PricingCalculatorBuilder.SyncVariantCost(product, main, entry.CalculateCostArs(settings.UsdtRate), unitOfWork, currentUser.UserId);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return PricingCalculatorBuilder.Build(product, entry, settings);
    }
}

// ── Precio de venta a mano ─────────────────────────────────────────────────────
public record VariantPriceDto(Guid VariantId, decimal Price);

/// <summary>
/// Fija a mano el precio de venta desde la calculadora, sin tocar el margen (puede quedar "desfasado").
/// Frasco: <see cref="PriceArs"/> va a la variante principal y al precio base.
/// Decant: <see cref="VariantPrices"/> por tamaño; el precio base queda igual al del tamaño más chico.
/// </summary>
public record SetManualSalePriceCommand(Guid ProductId, decimal? PriceArs, List<VariantPriceDto>? VariantPrices) : IRequest;

public class SetManualSalePriceCommandValidator : AbstractValidator<SetManualSalePriceCommand>
{
    public SetManualSalePriceCommandValidator()
    {
        RuleFor(x => x.PriceArs).GreaterThan(0).When(x => x.PriceArs.HasValue)
            .WithMessage("El precio de venta tiene que ser mayor a 0.");
        RuleForEach(x => x.VariantPrices).ChildRules(v =>
            v.RuleFor(i => i.Price).GreaterThan(0).WithMessage("El precio de venta tiene que ser mayor a 0."));
        RuleFor(x => x).Must(x => x.PriceArs.HasValue || x.VariantPrices is { Count: > 0 })
            .WithMessage("Indicá al menos un precio de venta.");
    }
}

public class SetManualSalePriceCommandHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : IRequestHandler<SetManualSalePriceCommand>
{
    public async Task Handle(SetManualSalePriceCommand request, CancellationToken ct)
    {
        var product = await products.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");
        var currency = product.BasePrice.Currency;

        if (product.IsDecant)
        {
            foreach (var vp in request.VariantPrices ?? [])
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == vp.VariantId)
                    ?? throw new NotFoundException($"Variante '{vp.VariantId}' no encontrada en el producto.");
                variant.UpdatePrice(Money.Of(vp.Price, currency));
            }

            var smallest = PricingCalculatorBuilder.DecantSizes(product).Select(s => s.Variant).FirstOrDefault();
            if (smallest?.Price is { } smallestPrice)
                PricingCalculatorBuilder.SetBasePrice(product, smallestPrice.Amount);
        }
        else if (request.PriceArs is { } price)
        {
            PricingCalculatorBuilder.MainVariant(product)?.UpdatePrice(Money.Of(price, currency));
            PricingCalculatorBuilder.SetBasePrice(product, price);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}

// ── Aplicar el sugerido ────────────────────────────────────────────────────────
/// <summary>
/// Pasa el precio sugerido al catálogo (precio base y de las variantes) de los productos indicados.
/// Un frasco que nunca se cargó en la calculadora guarda la estimación como su entrada, para que el
/// precio no cambie solo si después cambia la cotización. Devuelve cuántos productos se actualizaron.
/// </summary>
public record ApplySuggestedPriceCommand(List<Guid> ProductIds) : IRequest<int>;

public class ApplySuggestedPriceCommandHandler(
    IProductRepository products,
    IPricingCalculatorEntryRepository entries,
    IAppSettingRepository settingsRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<ApplySuggestedPriceCommand, int>
{
    public async Task<int> Handle(ApplySuggestedPriceCommand request, CancellationToken ct)
    {
        if (request.ProductIds is not { Count: > 0 }) return 0;

        var settings = await PricingSettings.LoadAsync(settingsRepository, ct);
        var updated = 0;

        foreach (var productId in request.ProductIds.Distinct())
        {
            var product = await products.GetWithVariantsAsync(productId, ct);
            if (product is null) continue;
            var currency = product.BasePrice.Currency;
            var entry = await entries.GetByProductIdAsync(product.Id, ct);

            if (product.IsDecant)
            {
                var (defType, defValue) = PricingSettings.MarginFor(PricingSettings.Decants, settings);
                var sizes = PricingCalculatorBuilder.PriceDecant(
                    product, entry?.MarginType ?? defType, entry?.MarginValue ?? defValue, settings)
                    .Where(s => s.Suggested > 0)
                    .ToList();
                if (sizes.Count == 0) continue;

                foreach (var size in sizes)
                {
                    size.Variant.UpdatePrice(Money.Of(size.Suggested, currency));
                    PricingCalculatorBuilder.SyncVariantCost(product, size.Variant, size.Cost, unitOfWork, currentUser.UserId);
                }
                PricingCalculatorBuilder.SetBasePrice(product, sizes[0].Suggested);
                updated++;
                continue;
            }

            var hadEntry = entry is not null;
            entry ??= PricingCalculatorBuilder.EstimateEntry(product, settings);
            if (entry is null) continue;   // sin costo: no hay sugerido

            var suggested = entry.CalculateSuggestedPriceArs(settings.UsdtRate);
            if (suggested <= 0) continue;

            if (!hadEntry) entries.Add(entry);

            var main = PricingCalculatorBuilder.MainVariant(product);
            main?.UpdatePrice(Money.Of(suggested, currency));
            // El costo solo se pisa si salió de datos cargados en la calculadora; la estimación viene
            // justamente del costo actual de la variante.
            if (hadEntry && main is not null)
                PricingCalculatorBuilder.SyncVariantCost(product, main, entry.CalculateCostArs(settings.UsdtRate), unitOfWork, currentUser.UserId);
            PricingCalculatorBuilder.SetBasePrice(product, suggested);
            updated++;
        }

        if (updated > 0) await unitOfWork.SaveChangesAsync(ct);
        return updated;
    }
}
