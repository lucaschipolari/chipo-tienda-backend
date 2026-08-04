using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Combos.DTOs;
using ChipoBackend.Application.Features.Products;
using ChipoBackend.Domain.Entities.Combos;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Combos.Commands.UpdateCombo;

public record UpdateComboCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    decimal Price,
    string Currency,
    List<ComboItemRequest> Items
) : IRequest;

public class UpdateComboCommandValidator : AbstractValidator<UpdateComboCommand>
{
    public UpdateComboCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("El combo debe tener al menos un producto.");
    }
}

public class UpdateComboCommandHandler(
    IComboRepository comboRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateComboCommand>
{
    public async Task Handle(UpdateComboCommand request, CancellationToken ct)
    {
        var combo = await comboRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Combo", request.Id);

        var slug = ComboMapper.Slugify(request.Name);
        if (await comboRepository.SlugExistsAsync(slug, combo.Id, ct))
            slug = $"{slug}-{Guid.NewGuid().ToString("n")[..6]}";

        var imageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : ImageUrlHelper.Normalize(request.ImageUrl);
        combo.Update(request.Name, slug, Money.Of(request.Price, request.Currency), request.Description, imageUrl);
        combo.ClearItems();
        foreach (var it in request.Items)
            combo.AddItem(it.ProductId, it.VariantId, it.Quantity);
        // Forzar Added en los ítems nuevos (mismo motivo que en órdenes de compra).
        foreach (var it in combo.Items)
            unitOfWork.Add(it);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
