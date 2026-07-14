using System.Text.Json;
using ChipoBackend.Domain.Entities.Config;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Settings;

/// <summary>Costo del frasquito (envase del decant) por tamaño en ml.</summary>
public record VialCostDto(int Ml, decimal Cost);

public static class VialCostSettings
{
    public const string Key = "decant_vial_costs";

    /// <summary>Parsea el JSON guardado ({"5":1600,"10":1800}) a un diccionario ml -> costo.</summary>
    public static Dictionary<int, decimal> Parse(string? json)
    {
        var result = new Dictionary<int, decimal>();
        if (string.IsNullOrWhiteSpace(json)) return result;
        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
            if (raw != null)
                foreach (var kv in raw)
                    if (int.TryParse(kv.Key, out var ml)) result[ml] = kv.Value;
        }
        catch { /* ignora JSON inválido */ }
        return result;
    }
}

// ── Query ──────────────────────────────────────────────────────────────────────
public record GetVialCostsQuery : IRequest<List<VialCostDto>>;

public class GetVialCostsQueryHandler(IAppSettingRepository settings)
    : IRequestHandler<GetVialCostsQuery, List<VialCostDto>>
{
    public async Task<List<VialCostDto>> Handle(GetVialCostsQuery request, CancellationToken ct)
    {
        var s = await settings.GetAsync(VialCostSettings.Key, ct);
        return VialCostSettings.Parse(s?.Value)
            .OrderBy(kv => kv.Key)
            .Select(kv => new VialCostDto(kv.Key, kv.Value))
            .ToList();
    }
}

// ── Command ────────────────────────────────────────────────────────────────────
public record SetVialCostsCommand(List<VialCostDto> Items) : IRequest;

public class SetVialCostsCommandHandler(
    IAppSettingRepository settings,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetVialCostsCommand>
{
    public async Task Handle(SetVialCostsCommand request, CancellationToken ct)
    {
        var map = (request.Items ?? [])
            .Where(i => i.Ml > 0)
            .ToDictionary(i => i.Ml.ToString(), i => i.Cost);
        var json = JsonSerializer.Serialize(map);

        var existing = await settings.GetAsync(VialCostSettings.Key, ct);
        if (existing is null)
            settings.Add(AppSetting.Create(VialCostSettings.Key, json));
        else
            existing.SetValue(json);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
