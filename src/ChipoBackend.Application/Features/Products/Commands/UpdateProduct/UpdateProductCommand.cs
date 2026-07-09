using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Products.DTOs;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Slug,
    decimal BasePrice,
    string Currency,
    decimal? CompareAtPrice,
    string? Description,
    bool IsFeatured,
    List<string> Tags,
    OlfactoryProfileDto? Olfactory = null
) : IRequest;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("La categoría es requerida.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(250)
            .Matches(@"^[a-z0-9\-]+$").WithMessage("El slug solo puede contener letras minúsculas, números y guiones.");
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.CompareAtPrice).GreaterThan(x => x.BasePrice)
            .When(x => x.CompareAtPrice.HasValue);
    }
}

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Producto '{request.Id}' no encontrado.");

        _ = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException($"Categoría '{request.CategoryId}' no encontrada.");

        // Verificar unicidad de slug (excluyendo el producto actual)
        if (await productRepository.SlugExistsAsync(request.Slug, request.Id, ct))
            throw new ConflictException($"El slug '{request.Slug}' ya está en uso.");

        var basePrice = Money.Of(request.BasePrice, request.Currency);
        var compareAtPrice = request.CompareAtPrice.HasValue
            ? Money.Of(request.CompareAtPrice.Value, request.Currency)
            : null;

        product.Update(request.Name, request.Slug, request.CategoryId,
            request.Description, basePrice, compareAtPrice, request.IsFeatured, request.Tags ?? []);

        if (request.Olfactory is { } olf)
            product.SetOlfactoryProfile(olf.TopNotes, olf.HeartNotes, olf.BaseNotes, olf.Intensity, olf.Longevity, olf.Seasons, olf.Occasions);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
