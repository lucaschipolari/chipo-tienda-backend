using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Infrastructure.Services;

public class PromotionEngine(IPromotionRepository promotionRepo, ICouponRepository couponRepo) : IPromotionEngine
{
    public async Task<PromotionCalculationResult> CalculateAsync(PromotionCalculationRequest request, CancellationToken ct = default)
    {
        var original = request.SubTotal;
        var messages = new List<string>();
        var appliedPromotions = new List<AppliedPromotionDto>();
        var currency = request.Currency;

        // 1. Load all active valid promotions
        var activePromotions = await promotionRepo.GetActiveValidAsync(ct);

        // 2. Evaluate each promotion
        var stackable = new List<(Promotion p, decimal saving)>();
        var nonStackable = new List<(Promotion p, decimal saving)>();

        foreach (var promo in activePromotions)
        {
            var saving = EvaluatePromotion(promo, request);
            if (saving <= 0) continue;
            if (promo.IsStackable) stackable.Add((promo, saving));
            else nonStackable.Add((promo, saving));
        }

        // 3. Pick best combination
        decimal promotionDiscount = 0;

        // Sum all stackable
        var stackableTotal = stackable.Sum(x => x.saving);

        // Best non-stackable
        decimal bestNonStackable = 0;
        (Promotion p, decimal saving) bestNonStackablePromo = default;
        if (nonStackable.Count > 0)
        {
            var best = nonStackable.OrderByDescending(x => x.saving).First();
            bestNonStackable = best.saving;
            bestNonStackablePromo = best;
        }

        // Use stackable + best non-stackable (they don't conflict if they're different types)
        promotionDiscount = stackableTotal + bestNonStackable;
        promotionDiscount = Math.Min(promotionDiscount, original); // cap at 100%

        foreach (var (p, s) in stackable)
            appliedPromotions.Add(new AppliedPromotionDto(p.Id, p.Name, p.Badge, Math.Round(s, 2)));
        if (bestNonStackablePromo != default)
            appliedPromotions.Add(new AppliedPromotionDto(bestNonStackablePromo.p.Id, bestNonStackablePromo.p.Name, bestNonStackablePromo.p.Badge, Math.Round(bestNonStackable, 2)));

        // 4. Coupon
        decimal couponDiscount = 0;
        AppliedCouponDto? appliedCoupon = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var coupon = await couponRepo.GetWithUsagesAsync(request.CouponCode, ct);
            if (coupon == null)
            {
                messages.Add("Cupón no encontrado.");
            }
            else if (!coupon.IsCurrentlyValid)
            {
                messages.Add("El cupón no es válido o ha expirado.");
            }
            else
            {
                var afterPromotions = Math.Max(0, original - promotionDiscount);
                if (coupon.MinOrderAmount != null && afterPromotions < coupon.MinOrderAmount.Amount)
                {
                    messages.Add($"El monto mínimo para este cupón es {coupon.MinOrderAmount}.");
                }
                else
                {
                    var disc = coupon.CalculateDiscount(Money.Of(afterPromotions, currency));
                    couponDiscount = disc.Amount;
                    appliedCoupon = new AppliedCouponDto(coupon.Code, coupon.Type.ToString(), Math.Round(couponDiscount, 2));
                }
            }
        }

        var totalSavings = promotionDiscount + couponDiscount;
        var finalTotal = Math.Max(0, original - totalSavings);

        return new PromotionCalculationResult(
            OriginalTotal: Math.Round(original, 2),
            PromotionDiscountAmount: Math.Round(promotionDiscount, 2),
            CouponDiscountAmount: Math.Round(couponDiscount, 2),
            FinalTotal: Math.Round(finalTotal, 2),
            TotalSavings: Math.Round(totalSavings, 2),
            AppliedPromotions: appliedPromotions,
            AppliedCoupon: appliedCoupon,
            IsValid: true,
            Messages: messages);
    }

    private static decimal EvaluatePromotion(Promotion promo, PromotionCalculationRequest request)
    {
        var items = request.Items;
        var subtotal = request.SubTotal;

        return promo.Type switch
        {
            PromotionType.Product or PromotionType.Flash or PromotionType.HappyHour =>
                EvaluateProductPromotion(promo, items),
            PromotionType.Category =>
                EvaluateCategoryPromotion(promo, items),
            PromotionType.BuyXGetY =>
                EvaluateBuyXGetY(promo, items),
            PromotionType.MinAmount =>
                EvaluateMinAmount(promo, subtotal),
            PromotionType.Combo =>
                EvaluateCombo(promo, items),
            _ => 0,
        };
    }

    private static decimal EvaluateProductPromotion(Promotion promo, List<PromotionCartItem> items)
    {
        var productIds = promo.Products.Select(p => p.ProductId).ToHashSet();
        var eligible = items.Where(i => productIds.Contains(i.ProductId)).ToList();
        if (!eligible.Any()) return 0;
        var eligible_total = eligible.Sum(i => i.Quantity * i.UnitPrice);
        return promo.DiscountType == DiscountType.Percentage
            ? eligible_total * (promo.DiscountValue / 100m)
            : Math.Min(promo.DiscountValue, eligible_total);
    }

    private static decimal EvaluateCategoryPromotion(Promotion promo, List<PromotionCartItem> items)
    {
        var catIds = promo.Categories.Select(c => c.CategoryId).ToHashSet();
        var eligible = items.Where(i => i.CategoryId.HasValue && catIds.Contains(i.CategoryId.Value)).ToList();
        if (!eligible.Any()) return 0;
        var eligible_total = eligible.Sum(i => i.Quantity * i.UnitPrice);
        return promo.DiscountType == DiscountType.Percentage
            ? eligible_total * (promo.DiscountValue / 100m)
            : Math.Min(promo.DiscountValue, eligible_total);
    }

    private static decimal EvaluateBuyXGetY(Promotion promo, List<PromotionCartItem> items)
    {
        if (!promo.BuyQuantity.HasValue || !promo.GetQuantity.HasValue) return 0;
        var productIds = promo.Products.Select(p => p.ProductId).ToHashSet();
        var eligible = items.Where(i => productIds.Contains(i.ProductId)).ToList();
        if (!eligible.Any()) return 0;
        var totalQty = eligible.Sum(i => i.Quantity);
        if (totalQty < promo.BuyQuantity.Value) return 0;
        var freeQty = (totalQty / (promo.BuyQuantity.Value + promo.GetQuantity.Value)) * promo.GetQuantity.Value;
        var minPrice = eligible.Min(i => i.UnitPrice);
        return freeQty * minPrice;
    }

    private static decimal EvaluateMinAmount(Promotion promo, decimal subtotal)
    {
        if (promo.MinOrderAmount.HasValue && subtotal < promo.MinOrderAmount.Value) return 0;
        return promo.DiscountType == DiscountType.Percentage
            ? subtotal * (promo.DiscountValue / 100m)
            : Math.Min(promo.DiscountValue, subtotal);
    }

    private static decimal EvaluateCombo(Promotion promo, List<PromotionCartItem> items)
    {
        var productIds = promo.Products.Select(p => p.ProductId).ToHashSet();
        var cartProductIds = items.Select(i => i.ProductId).ToHashSet();
        if (!productIds.All(id => cartProductIds.Contains(id))) return 0; // all combo items must be in cart
        var comboItems = items.Where(i => productIds.Contains(i.ProductId)).ToList();
        var originalTotal = comboItems.Sum(i => i.UnitPrice);
        if (promo.ComboPrice.HasValue)
            return Math.Max(0, originalTotal - promo.ComboPrice.Value);
        return promo.DiscountType == DiscountType.Percentage
            ? originalTotal * (promo.DiscountValue / 100m)
            : Math.Min(promo.DiscountValue, originalTotal);
    }
}
