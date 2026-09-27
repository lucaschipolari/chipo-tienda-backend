using System.Text.RegularExpressions;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Queries.GetDecantSales;

public record DecantPointDto(string Label, int Units, int Ml);
public record DecantTopDto(string ProductName, int Units);
public record DecantSalesDto(
    string Granularity,
    int TotalUnits,
    int TotalMl,
    List<DecantPointDto> Series,
    List<DecantTopDto> Top
);

/// <summary>Decants vendidos agrupados por día (últimos 30), semana (últimas 12) o mes (últimos 12).</summary>
public record GetDecantSalesQuery(string Granularity = "day") : IRequest<DecantSalesDto>;

public class GetDecantSalesQueryHandler(
    ISaleRepository saleRepository,
    IProductRepository productRepository
) : IRequestHandler<GetDecantSalesQuery, DecantSalesDto>
{
    private static readonly string[] MesesAbrev =
        ["ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic"];

    public async Task<DecantSalesDto> Handle(GetDecantSalesQuery request, CancellationToken ct)
    {
        var gran = request.Granularity?.ToLowerInvariant() switch
        {
            "week" => "week",
            "month" => "month",
            _ => "day"
        };

        var today = DateTime.UtcNow.Date;
        DateTime from = gran switch
        {
            "week" => today.AddDays(-7 * 11),   // 12 semanas
            "month" => new DateTime(today.Year, today.Month, 1).AddMonths(-11), // 12 meses
            _ => today.AddDays(-29)             // 30 días
        };
        var to = today.AddDays(1).AddTicks(-1); // fin del día de hoy

        // Mapa de variantes decant -> ml por unidad, y set de productos decant
        var products = await productRepository.GetAllWithVariantsAndCategoryAsync(ct);
        var mlByVariant = new Dictionary<Guid, int>();
        var decantProducts = new HashSet<Guid>();
        foreach (var p in products.Where(p => p.IsDecant))
        {
            decantProducts.Add(p.Id);
            foreach (var v in p.Variants)
                mlByVariant[v.Id] = ParseMl(v.Attributes);
        }

        var sales = await saleRepository.GetWithItemsByDateRangeAsync(
            DateTime.SpecifyKind(from, DateTimeKind.Utc),
            DateTime.SpecifyKind(to, DateTimeKind.Utc), ct);

        // Buckets ordenados (zero-fill)
        var buckets = BuildBuckets(gran, today);
        var unitsByKey = buckets.ToDictionary(b => b.Key, _ => 0);
        var mlByKey = buckets.ToDictionary(b => b.Key, _ => 0);
        var topUnits = new Dictionary<string, int>();
        int totalUnits = 0, totalMl = 0;

        foreach (var sale in sales)
        {
            var key = KeyFor(gran, sale.CreatedAt);
            if (!unitsByKey.ContainsKey(key)) continue; // fuera de rango
            foreach (var item in sale.Items)
            {
                if (!decantProducts.Contains(item.ProductId)) continue;
                var ml = (mlByVariant.TryGetValue(item.VariantId, out var m) ? m : 0) * item.Quantity;
                unitsByKey[key] += item.Quantity;
                mlByKey[key] += ml;
                totalUnits += item.Quantity;
                totalMl += ml;
                topUnits[item.ProductName] = topUnits.GetValueOrDefault(item.ProductName) + item.Quantity;
            }
        }

        var series = buckets
            .Select(b => new DecantPointDto(b.Label, unitsByKey[b.Key], mlByKey[b.Key]))
            .ToList();

        var top = topUnits
            .OrderByDescending(kv => kv.Value)
            .Take(7)
            .Select(kv => new DecantTopDto(kv.Key, kv.Value))
            .ToList();

        return new DecantSalesDto(gran, totalUnits, totalMl, series, top);
    }

    // Lista ordenada de buckets (clave + etiqueta) para zero-fill.
    private static List<(string Key, string Label)> BuildBuckets(string gran, DateTime today)
    {
        var list = new List<(string, string)>();
        if (gran == "month")
        {
            var start = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
            for (int i = 0; i < 12; i++)
            {
                var d = start.AddMonths(i);
                list.Add(($"{d.Year:D4}-{d.Month:D2}", $"{MesesAbrev[d.Month - 1]} {d:yy}"));
            }
        }
        else if (gran == "week")
        {
            var startWeek = StartOfWeek(today).AddDays(-7 * 11);
            for (int i = 0; i < 12; i++)
            {
                var d = startWeek.AddDays(7 * i);
                list.Add(($"{d:yyyy-MM-dd}", $"{d:dd}/{d.Month:D2}"));
            }
        }
        else
        {
            var start = today.AddDays(-29);
            for (int i = 0; i < 30; i++)
            {
                var d = start.AddDays(i);
                list.Add(($"{d:yyyy-MM-dd}", $"{d:dd} {MesesAbrev[d.Month - 1]}"));
            }
        }
        return list;
    }

    private static string KeyFor(string gran, DateTime dt)
    {
        var d = dt.Date;
        return gran switch
        {
            "month" => $"{d.Year:D4}-{d.Month:D2}",
            "week" => $"{StartOfWeek(d):yyyy-MM-dd}",
            _ => $"{d:yyyy-MM-dd}"
        };
    }

    private static DateTime StartOfWeek(DateTime d)
    {
        int diff = ((int)d.DayOfWeek + 6) % 7; // lunes = inicio
        return d.Date.AddDays(-diff);
    }

    private static int ParseMl(Dictionary<string, string> attributes)
    {
        if (attributes == null) return 0;
        foreach (var v in attributes.Values)
        {
            var m = Regex.Match(v ?? "", @"(\d+)\s*ml", RegexOptions.IgnoreCase);
            if (m.Success) return int.Parse(m.Groups[1].Value);
        }
        return 0;
    }
}
