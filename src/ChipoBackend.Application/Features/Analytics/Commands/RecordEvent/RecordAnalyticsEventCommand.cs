using ChipoBackend.Domain.Entities.Analytics;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Analytics.Commands.RecordEvent;

/// <summary>
/// Registra un evento de interacción anónimo. Tipos: "view", "cart",
/// "favorite", "search". Pensado para llamarse desde la tienda sin auth.
/// </summary>
public record RecordAnalyticsEventCommand(
    string Type,
    Guid? ProductId,
    string? SearchTerm,
    int? ResultCount,
    string? SessionId
) : IRequest;

public class RecordAnalyticsEventCommandValidator : AbstractValidator<RecordAnalyticsEventCommand>
{
    private static readonly string[] Valid = ["view", "cart", "favorite", "search"];

    public RecordAnalyticsEventCommandValidator()
    {
        RuleFor(x => x.Type).NotEmpty()
            .Must(t => Valid.Contains(t.ToLowerInvariant()))
            .WithMessage("Tipo de evento inválido.");
        RuleFor(x => x.ProductId).NotNull()
            .When(x => x.Type is not null && x.Type.ToLowerInvariant() is "view" or "cart" or "favorite")
            .WithMessage("El producto es requerido para este evento.");
        RuleFor(x => x.SearchTerm).NotEmpty()
            .When(x => x.Type is not null && x.Type.ToLowerInvariant() == "search")
            .WithMessage("El término de búsqueda es requerido.");
    }
}

public class RecordAnalyticsEventCommandHandler(
    IAnalyticsEventRepository repository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RecordAnalyticsEventCommand>
{
    public async Task Handle(RecordAnalyticsEventCommand request, CancellationToken ct)
    {
        var kind = request.Type.ToLowerInvariant();
        AnalyticsEvent ev = kind switch
        {
            "view"     => AnalyticsEvent.ForProduct(AnalyticsEventType.ProductView, request.ProductId!.Value, request.SessionId),
            "cart"     => AnalyticsEvent.ForProduct(AnalyticsEventType.AddToCart, request.ProductId!.Value, request.SessionId),
            "favorite" => AnalyticsEvent.ForProduct(AnalyticsEventType.Favorite, request.ProductId!.Value, request.SessionId),
            "search"   => AnalyticsEvent.ForSearch(request.SearchTerm!, request.ResultCount ?? 0, request.SessionId),
            _          => throw new InvalidOperationException("Tipo inválido."),
        };

        repository.Add(ev);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
