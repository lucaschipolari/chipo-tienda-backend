using System.Text.RegularExpressions;
using ChipoBackend.Application.Features.Combos.DTOs;
using ChipoBackend.Domain.Entities.Combos;
using ChipoBackend.Domain.Interfaces.Repositories;

namespace ChipoBackend.Application.Features.Combos;

public static class ComboMapper
{
    public static async Task<ComboDto> ToDtoAsync(Combo c, IProductRepository products, CancellationToken ct)
    {
        var items = new List<ComboItemDto>();
        decimal originalTotal = 0m;

        foreach (var it in c.Items)
        {
            var p = await products.GetWithVariantsAsync(it.ProductId, ct);
            var v = p?.Variants.FirstOrDefault(x => x.Id == it.VariantId);
            var unit = v?.Price?.Amount is { } vp && vp > 0 ? vp : (p?.BasePrice.Amount ?? 0m);
            var label = v != null
                ? string.Join(" / ", v.Attributes.Select(kv => kv.Value))
                : "";
            var img = p?.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.Url;
            items.Add(new ComboItemDto(
                it.ProductId, it.VariantId,
                p?.Name ?? "(producto eliminado)",
                string.IsNullOrWhiteSpace(label) ? "Único" : label,
                it.Quantity, unit, p?.IsDecant ?? false, img));
            originalTotal += unit * it.Quantity;
        }

        return new ComboDto(
            c.Id, c.Name, c.Slug, c.Description, c.ImageUrl,
            c.Price.Amount, originalTotal, c.Price.Currency, c.IsActive,
            c.Items.Count, items, c.CreatedAt);
    }

    public static string Slugify(string name)
    {
        var s = name.Trim().ToLowerInvariant();
        s = s.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"\s+", "-").Trim('-');
        return string.IsNullOrEmpty(s) ? Guid.NewGuid().ToString("n")[..8] : s;
    }
}
