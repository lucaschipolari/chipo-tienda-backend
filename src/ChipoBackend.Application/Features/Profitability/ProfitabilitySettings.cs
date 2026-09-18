using System.Text.Json;
using ChipoBackend.Domain.Entities.Config;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Profitability;

/// <summary>
/// Configuración global de rentabilidad (margen objetivo general, umbrales de estado y redondeo).
/// Se guarda como JSON en AppSetting, igual que los costos de frasquito.
/// </summary>
public record ProfitabilitySettingsDto(
    decimal TargetMarginPct,   // margen objetivo general (0–100)
    decimal CriticalBandPct,   // cuántos puntos por debajo del objetivo pasa de amarillo a rojo
    int RoundingStep,          // paso de redondeo del precio sugerido (0 = sin redondeo)
    string RoundingMode        // "nearest" | "up" | "down"
)
{
    public static ProfitabilitySettingsDto Default => new(35m, 10m, 500, "nearest");
}

public static class ProfitabilitySettings
{
    public const string Key = "profitability_settings";

    public static ProfitabilitySettingsDto Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return ProfitabilitySettingsDto.Default;
        try
        {
            var dto = JsonSerializer.Deserialize<ProfitabilitySettingsDto>(json,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            return dto is null ? ProfitabilitySettingsDto.Default : Normalize(dto);
        }
        catch { return ProfitabilitySettingsDto.Default; }
    }

    public static ProfitabilitySettingsDto Normalize(ProfitabilitySettingsDto d)
    {
        var target = Math.Clamp(d.TargetMarginPct, 0m, 99.99m);
        var band = d.CriticalBandPct < 0 ? 0m : d.CriticalBandPct;
        var step = d.RoundingStep < 0 ? 0 : d.RoundingStep;
        var mode = d.RoundingMode?.ToLowerInvariant() switch
        {
            "up" => "up",
            "down" => "down",
            _ => "nearest"
        };
        return new ProfitabilitySettingsDto(target, band, step, mode);
    }
}

// ── Query ────────────────────────────────────────────────────────────────────
public record GetProfitabilitySettingsQuery : IRequest<ProfitabilitySettingsDto>;

public class GetProfitabilitySettingsQueryHandler(IAppSettingRepository settings)
    : IRequestHandler<GetProfitabilitySettingsQuery, ProfitabilitySettingsDto>
{
    public async Task<ProfitabilitySettingsDto> Handle(GetProfitabilitySettingsQuery request, CancellationToken ct)
    {
        var s = await settings.GetAsync(ProfitabilitySettings.Key, ct);
        return ProfitabilitySettings.Parse(s?.Value);
    }
}

// ── Command ──────────────────────────────────────────────────────────────────
public record SetProfitabilitySettingsCommand(ProfitabilitySettingsDto Settings) : IRequest;

public class SetProfitabilitySettingsCommandValidator : AbstractValidator<SetProfitabilitySettingsCommand>
{
    public SetProfitabilitySettingsCommandValidator()
    {
        RuleFor(x => x.Settings.TargetMarginPct).InclusiveBetween(0m, 99.99m)
            .WithMessage("El margen objetivo debe estar entre 0 y 99,99%.");
        RuleFor(x => x.Settings.CriticalBandPct).GreaterThanOrEqualTo(0m);
        RuleFor(x => x.Settings.RoundingStep).GreaterThanOrEqualTo(0);
    }
}

public class SetProfitabilitySettingsCommandHandler(
    IAppSettingRepository settings,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetProfitabilitySettingsCommand>
{
    public async Task Handle(SetProfitabilitySettingsCommand request, CancellationToken ct)
    {
        var normalized = ProfitabilitySettings.Normalize(request.Settings);
        var json = JsonSerializer.Serialize(normalized, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var existing = await settings.GetAsync(ProfitabilitySettings.Key, ct);
        if (existing is null)
            settings.Add(AppSetting.Create(ProfitabilitySettings.Key, json));
        else
            existing.SetValue(json);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
