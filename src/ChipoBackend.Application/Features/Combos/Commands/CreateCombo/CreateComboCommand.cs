using ChipoBackend.Application.Features.Combos.DTOs;
using ChipoBackend.Application.Features.Products;
using ChipoBackend.Domain.Entities.Combos;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Combos.Commands.CreateCombo;

public record CreateComboCommand(
    string Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    string Currency,
    List<ComboItemRequest> Items
) : IRequest<Guid>;

public class CreateComboCommandValidator : AbstractValidator<CreateComboCommand>
{
    public CreateComboCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("El precio del combo debe ser mayor a 0.");
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Items).NotEmpty().WithMessage("El combo debe tener al menos un producto.");
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.ProductId).NotEmpty();
            i.RuleFor(x => x.VariantId).NotEmpty();
            i.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}

public class CreateComboCommandHandler(
    IComboRepository comboRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateComboCommand, Guid>
{
    public async Task<Guid> Handle(CreateComboCommand request, CancellationToken ct)
    {
        var slug = ComboMapper.Slugify(request.Name);
        if (await comboRepository.SlugExistsAsync(slug, null, ct))
            slug = $"{slug}-{Guid.NewGuid().ToString("n")[..6]}";

        var imageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : ImageUrlHelper.Normalize(request.ImageUrl);
        var combo = Combo.Create(request.Name, slug, Money.Of(request.Price, request.Currency),
            request.Description, imageUrl);
        foreach (var it in request.Items)
            combo.AddItem(it.ProductId, it.VariantId, it.Quantity);

        await comboRepository.AddAsync(combo, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return combo.Id;
    }
}
