using ChipoBackend.Application.Features.PricingCalculator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/pricing-calculator")]
[Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
public class PricingCalculatorController : BaseApiController
{
    /// <summary>Panel de la calculadora: ajustes y desglose de precio por producto. group: arabes | disenador | nicho | decants.</summary>
    [HttpGet]
    public async Task<ActionResult<PricingCalculatorOverviewDto>> Get(
        [FromQuery] string? group, [FromQuery] string? search, CancellationToken ct)
        => Ok(await Mediator.Send(new GetPricingCalculatorQuery(group, search), ct));

    /// <summary>Cotización USDT, gastos de compra, márgenes por grupo y extras de packaging.</summary>
    [HttpPut("settings")]
    public async Task<ActionResult<PricingSettingsDto>> UpdateSettings(
        [FromBody] UpdatePricingSettingsCommand command, CancellationToken ct)
        => Ok(await Mediator.Send(command, ct));

    /// <summary>Datos de compra (USD) y margen de un producto; actualiza el costo de sus variantes.</summary>
    [HttpPut("products/{productId:guid}")]
    public async Task<ActionResult<ProductPricingItemDto>> UpdateProduct(
        Guid productId, [FromBody] UpdateProductPricingRequest request, CancellationToken ct)
        => Ok(await Mediator.Send(new UpdateProductPricingEntryCommand(
            productId, request.CostUsdOrigin, request.PercentageCommission, request.FixedCommission,
            request.ShippingCost, request.MarginType, request.MarginValue), ct));

    /// <summary>Precio de venta a mano (frasco: priceArs; decant: variantPrices por tamaño).</summary>
    [HttpPut("products/{productId:guid}/sale-price")]
    public async Task<IActionResult> SetSalePrice(
        Guid productId, [FromBody] SetSalePriceRequest request, CancellationToken ct)
    {
        await Mediator.Send(new SetManualSalePriceCommand(productId, request.PriceArs, request.VariantPrices), ct);
        return NoContent();
    }

    /// <summary>Pasa el precio sugerido al catálogo para los productos indicados.</summary>
    [HttpPost("apply-suggested-price")]
    public async Task<ActionResult<ApplySuggestedPriceResponse>> ApplySuggested(
        [FromBody] ApplySuggestedPriceRequest request, CancellationToken ct)
        => Ok(new ApplySuggestedPriceResponse(await Mediator.Send(new ApplySuggestedPriceCommand(request.ProductIds), ct)));
}

public record UpdateProductPricingRequest(
    decimal CostUsdOrigin,
    decimal PercentageCommission,
    decimal FixedCommission,
    decimal ShippingCost,
    string MarginType,
    decimal MarginValue);

public record SetSalePriceRequest(decimal? PriceArs, List<VariantPriceDto>? VariantPrices);

public record ApplySuggestedPriceRequest(List<Guid> ProductIds);

public record ApplySuggestedPriceResponse(int UpdatedProductsCount);
