using System.Text.Json;
using ChipoBackend.Domain.Entities.Config;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Settings;

/// <summary>Frasco físico (producto de stock) asociado a un tamaño de decant.</summary>
public record VialProductDto(int Ml, Guid ProductId);

/// <summary>
/// Mapea cada tamaño de decant (ml) al producto "frasco vacío" cuyo stock debe descontarse
/// automáticamente al vender un decant de ese tamaño. Se guarda como JSON en AppSetting:
/// { "5": "&lt;productId&gt;", "10": "&lt;productId&gt;" }.
/// </summary>
public static class DecantVialProducts
{
    public const string Key = "decant_vial_products";

    public static Dictionary<int, Guid> Parse(string? json)
    {
        var result = new Dictionary<int, Guid>();
        if (string.IsNullOrWhiteSpace(json)) return result;
        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (raw != null)
                foreach (var kv in raw)
                    if (int.TryParse(kv.Key, out var ml) && Guid.TryParse(kv.Value, out var pid))
                        result[ml] = pid;
        }
        catch { /* ignora JSON inválido */ }
        return result;
    }
}

// ── Query ──────────────────────────────────────────────────────────────────────
public record GetVialProductsQuery : IRequest<List<VialProductDto>>;

public class GetVialProductsQueryHandler(IAppSettingRepository settings)
    : IRequestHandler<GetVialProductsQuery, List<VialProductDto>>
{
    public async Task<List<VialProductDto>> Handle(GetVialProductsQuery request, CancellationToken ct)
    {
        var s = await settings.GetAsync(DecantVialProducts.Key, ct);
        return DecantVialProducts.Parse(s?.Value)
            .OrderBy(kv => kv.Key)
            .Select(kv => new VialProductDto(kv.Key, kv.Value))
            .ToList();
    }
}

// ── Command ────────────────────────────────────────────────────────────────────
public record SetVialProductsCommand(List<VialProductDto> Items) : IRequest;

public class SetVialProductsCommandHandler(
    IAppSettingRepository settings,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetVialProductsCommand>
{
    public async Task Handle(SetVialProductsCommand request, CancellationToken ct)
    {
        var map = (request.Items ?? [])
            .Where(i => i.Ml > 0 && i.ProductId != Guid.Empty)
            .GroupBy(i => i.Ml)
            .ToDictionary(g => g.Key.ToString(), g => g.Last().ProductId.ToString());
        var json = JsonSerializer.Serialize(map);

        var existing = await settings.GetAsync(DecantVialProducts.Key, ct);
        if (existing is null)
            settings.Add(AppSetting.Create(DecantVialProducts.Key, json));
        else
            existing.SetValue(json);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
