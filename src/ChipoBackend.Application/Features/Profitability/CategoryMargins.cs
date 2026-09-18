using System.Text.Json;
using ChipoBackend.Domain.Entities.Config;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Profitability;

/// <summary>Margen objetivo específico de una categoría.</summary>
public record CategoryMarginDto(Guid CategoryId, decimal TargetMarginPct);

/// <summary>
/// Márgenes objetivo por categoría (ej: Diseñador 40%, Nicho 45%, Decants 50%).
/// Se guardan como JSON en AppSetting ({ "&lt;guid&gt;": 40 }). Prioridad de margen:
/// producto → categoría → general.
/// </summary>
public static class CategoryMargins
{
    public const string Key = "category_margins";

    public static Dictionary<Guid, decimal> Parse(string? json)
    {
        var result = new Dictionary<Guid, decimal>();
        if (string.IsNullOrWhiteSpace(json)) return result;
        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
            if (raw != null)
                foreach (var kv in raw)
                    if (Guid.TryParse(kv.Key, out var id) && kv.Value is >= 0 and < 100)
                        result[id] = kv.Value;
        }
        catch { /* ignora JSON inválido */ }
        return result;
    }
}

// ── Query ────────────────────────────────────────────────────────────────────
public record GetCategoryMarginsQuery : IRequest<List<CategoryMarginDto>>;

public class GetCategoryMarginsQueryHandler(IAppSettingRepository settings)
    : IRequestHandler<GetCategoryMarginsQuery, List<CategoryMarginDto>>
{
    public async Task<List<CategoryMarginDto>> Handle(GetCategoryMarginsQuery request, CancellationToken ct)
    {
        var s = await settings.GetAsync(CategoryMargins.Key, ct);
        return CategoryMargins.Parse(s?.Value)
            .Select(kv => new CategoryMarginDto(kv.Key, kv.Value))
            .ToList();
    }
}

// ── Command ──────────────────────────────────────────────────────────────────
public record SetCategoryMarginsCommand(List<CategoryMarginDto> Items) : IRequest;

public class SetCategoryMarginsCommandValidator : AbstractValidator<SetCategoryMarginsCommand>
{
    public SetCategoryMarginsCommandValidator()
    {
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(m => m.CategoryId).NotEmpty();
            i.RuleFor(m => m.TargetMarginPct).InclusiveBetween(0m, 99.99m)
                .WithMessage("El margen de la categoría debe estar entre 0 y 99,99%.");
        });
    }
}

public class SetCategoryMarginsCommandHandler(
    IAppSettingRepository settings,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetCategoryMarginsCommand>
{
    public async Task Handle(SetCategoryMarginsCommand request, CancellationToken ct)
    {
        // Solo guardamos categorías con margen definido (>= 0). Quitar = no incluir.
        var map = (request.Items ?? [])
            .GroupBy(i => i.CategoryId)
            .ToDictionary(g => g.Key.ToString(), g => g.Last().TargetMarginPct);
        var json = JsonSerializer.Serialize(map);

        var existing = await settings.GetAsync(CategoryMargins.Key, ct);
        if (existing is null)
            settings.Add(AppSetting.Create(CategoryMargins.Key, json));
        else
            existing.SetValue(json);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
