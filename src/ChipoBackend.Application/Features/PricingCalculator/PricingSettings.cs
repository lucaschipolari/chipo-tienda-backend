using System.Globalization;
using System.Text.Json;
using ChipoBackend.Application.Features.Settings;
using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Entities.Config;
using ChipoBackend.Domain.Interfaces.Repositories;

namespace ChipoBackend.Application.Features.PricingCalculator;

/// <summary>
/// Ajustes de la Calculadora guardados en AppSetting (una clave por valor, prefijo "pricing_").
/// El costo de los frasquitos se lee del ajuste de Inventario (<see cref="VialCostSettings"/>).
/// </summary>
public static class PricingSettings
{
    public const string UsdtRateKey = "pricing_usdt_rate";
    public const string CommissionPercentageKey = "pricing_commission_percentage";
    public const string CommissionFixedStandardKey = "pricing_commission_fixed_standard";
    public const string CommissionFixedCheapKey = "pricing_commission_fixed_cheap";
    public const string ShippingCostKey = "pricing_shipping_cost";
    public const string PackagingExtrasKey = "pricing_decant_packaging_extras";

    public const string Arabes = "arabes";
    public const string Disenador = "disenador";
    public const string Nicho = "nicho";
    public const string Decants = "decants";

    /// <summary>Grupos de margen con su etiqueta y el margen que usan si nunca se configuró.</summary>
    public static readonly IReadOnlyList<(string Group, string Label, string Type, decimal Value)> Groups =
    [
        (Arabes, "Árabes", PricingFormula.FixedAmount, 20000m),
        (Disenador, "Diseñador", PricingFormula.Percentage, 35m),
        (Nicho, "Nicho", PricingFormula.Percentage, 45m),
        (Decants, "Decants", PricingFormula.Percentage, 60m),
    ];

    public static string MarginTypeKey(string group) => $"pricing_margin_{group}_type";
    public static string MarginValueKey(string group) => $"pricing_margin_{group}_value";

    /// <summary>Grupo de margen de un producto según su categoría (los decants siempre van a "decants").</summary>
    public static string GroupFor(bool isDecant, string? categorySlug)
    {
        var slug = (categorySlug ?? "").ToLowerInvariant();
        if (isDecant || slug.Contains("decant")) return Decants;
        if (slug.Contains("arabe") || slug.Contains("árabe")) return Arabes;
        if (slug.Contains("nicho")) return Nicho;
        return Disenador;
    }

    public static async Task<PricingSettingsDto> LoadAsync(IAppSettingRepository repo, CancellationToken ct)
    {
        async Task<decimal> Dec(string key, decimal fallback) =>
            ParseDecimal((await repo.GetAsync(key, ct))?.Value, fallback);

        var margins = new List<GroupMarginDto>();
        foreach (var (group, label, defType, defValue) in Groups)
        {
            var type = PricingFormula.NormalizeMarginType((await repo.GetAsync(MarginTypeKey(group), ct))?.Value) ?? defType;
            margins.Add(new GroupMarginDto(group, label, type, await Dec(MarginValueKey(group), defValue)));
        }

        var vialCosts = VialCostSettings.Parse((await repo.GetAsync(VialCostSettings.Key, ct))?.Value)
            .OrderBy(kv => kv.Key)
            .Select(kv => new VialCostDto(kv.Key, kv.Value))
            .ToList();

        return new PricingSettingsDto(
            UsdtRate: await Dec(UsdtRateKey, 1300m),
            CommissionPercentage: await Dec(CommissionPercentageKey, 1m),
            CommissionFixedStandard: await Dec(CommissionFixedStandardKey, 5m),
            CommissionFixedCheap: await Dec(CommissionFixedCheapKey, 3m),
            ShippingCost: await Dec(ShippingCostKey, 1.5m),
            Margins: margins,
            VialCosts: vialCosts,
            DecantPackagingExtras: ParseExtras((await repo.GetAsync(PackagingExtrasKey, ct))?.Value));
    }

    public static (string Type, decimal Value) MarginFor(string group, PricingSettingsDto settings)
    {
        var m = settings.Margins.FirstOrDefault(x => x.Group == group)
                ?? settings.Margins.First(x => x.Group == Disenador);
        return (m.MarginType, m.MarginValue);
    }

    /// <summary>Packaging (ARS) de un decant de <paramref name="ml"/>: frasquito de ese tamaño + extras.</summary>
    public static decimal PackagingFor(int ml, PricingSettingsDto settings)
    {
        var vial = settings.VialCosts.FirstOrDefault(v => v.Ml == ml)?.Cost ?? 0m;
        return vial + settings.DecantPackagingExtras.Sum(e => e.Cost);
    }

    /// <summary>Guarda un valor (crea la clave si no existe). No llama a SaveChanges.</summary>
    public static async Task SetAsync(IAppSettingRepository repo, string key, string value, CancellationToken ct)
    {
        var existing = await repo.GetAsync(key, ct);
        if (existing is null) repo.Add(AppSetting.Create(key, value));
        else existing.SetValue(value);
    }

    public static string Format(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    public static string SerializeExtras(IEnumerable<PackagingExtraDto> extras) =>
        JsonSerializer.Serialize(extras, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static List<PackagingExtraDto> ParseExtras(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            var parsed = JsonSerializer.Deserialize<List<PackagingExtraDto>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return parsed?.Where(e => e is not null && !string.IsNullOrWhiteSpace(e.Name) && e.Cost >= 0).ToList() ?? [];
        }
        catch { return []; }
    }

    private static decimal ParseDecimal(string? raw, decimal fallback) =>
        decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : fallback;
}
