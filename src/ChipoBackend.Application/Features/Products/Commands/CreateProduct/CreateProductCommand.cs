using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.Products.DTOs;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using System.Text.RegularExpressions;

namespace ChipoBackend.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string? Slug,
    string Sku,
    decimal BasePrice,
    string Currency,
    decimal? CompareAtPrice,
    string? Description,
    bool IsFeatured,
    List<string> Tags,
    List<CreateVariantRequest> InitialVariants,
    OlfactoryProfileDto? Olfactory = null,
    List<string>? ImageUrls = null
) : IRequest<Guid>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithMessage("La categoría es requerida.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).WithMessage("El nombre es requerido (máx 200 caracteres).");
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100).WithMessage("El SKU es requerido (máx 100 caracteres).");
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0).WithMessage("El precio base debe ser mayor o igual a 0.");
        RuleFor(x => x.Currency).NotEmpty().Length(3).WithMessage("La moneda debe ser un código ISO de 3 letras (ej. PEN, USD).");
        RuleFor(x => x.CompareAtPrice).GreaterThan(x => x.BasePrice)
            .When(x => x.CompareAtPrice.HasValue)
            .WithMessage("El precio de comparación debe ser mayor al precio base.");
        RuleFor(x => x.InitialVariants).NotEmpty().WithMessage("Debe incluir al menos una variante.");
        RuleForEach(x => x.InitialVariants).ChildRules(v =>
        {
            v.RuleFor(r => r.Sku).NotEmpty().WithMessage("El SKU de variante es requerido.");
            v.RuleFor(r => r.InitialStock).GreaterThanOrEqualTo(0).WithMessage("El stock inicial no puede ser negativo.");
            v.RuleFor(r => r.MinStockThreshold).GreaterThanOrEqualTo(0).WithMessage("El umbral mínimo no puede ser negativo.");
        });
    }
}

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Verificar que la categoría existe
        var category = await categoryRepository.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException($"Categoría '{request.CategoryId}' no encontrada.");

        // Generar/validar slug
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : NormalizeSlug(request.Slug);

        // Verificar unicidad de slug y SKU
        if (await productRepository.SlugExistsAsync(slug, ct: ct))
            throw new ConflictException($"El slug '{slug}' ya está en uso.");
        if (await productRepository.SkuExistsAsync(request.Sku, ct: ct))
            throw new ConflictException($"El SKU '{request.Sku}' ya está en uso.");

        // Verificar SKUs de variantes únicos entre sí
        var variantSkus = request.InitialVariants.Select(v => v.Sku).ToList();
        if (variantSkus.Distinct(StringComparer.OrdinalIgnoreCase).Count() != variantSkus.Count)
            throw new ConflictException("Los SKUs de las variantes deben ser únicos entre sí.");

        // Crear producto
        var basePrice = Money.Of(request.BasePrice, request.Currency);
        var compareAtPrice = request.CompareAtPrice.HasValue
            ? Money.Of(request.CompareAtPrice.Value, request.Currency)
            : null;

        var product = Product.Create(request.CategoryId, request.Name, slug, request.Sku, basePrice, request.Description);
        product.Update(request.Name, slug, request.CategoryId, request.Description, basePrice, compareAtPrice, request.IsFeatured, request.Tags ?? []);

        if (request.Olfactory is { } olf)
            product.SetOlfactoryProfile(olf.TopNotes, olf.HeartNotes, olf.BaseNotes, olf.Intensity, olf.Longevity, olf.Seasons, olf.Occasions);

        // Imágenes por URL (se admiten links de Google Drive; se normalizan a URL directa)
        if (request.ImageUrls is { Count: > 0 } urls)
        {
            var order = 0;
            foreach (var raw in urls.Where(u => !string.IsNullOrWhiteSpace(u)))
                product.AddImage(ImageUrlHelper.Normalize(raw), displayOrder: order++);
        }

        unitOfWork.Add(product);

        // Agregar variantes y registrar movimientos iniciales
        foreach (var varReq in request.InitialVariants)
        {
            var varPrice = varReq.Price.HasValue ? Money.Of(varReq.Price.Value, request.Currency) : null;
            var varCompareAt = varReq.CompareAtPrice.HasValue ? Money.Of(varReq.CompareAtPrice.Value, request.Currency) : null;
            var varCost = varReq.Cost.HasValue ? Money.Of(varReq.Cost.Value, request.Currency) : null;
            var variant = product.AddVariant(varReq.Sku, varReq.Attributes ?? [], varReq.InitialStock, varPrice, varReq.MinStockThreshold, varCompareAt, varCost);
            unitOfWork.Add(variant);

            if (varReq.InitialStock > 0)
            {
                var movement = StockMovement.Create(
                    product.Id, variant.Id, MovementType.Initial,
                    varReq.InitialStock, 0, varReq.InitialStock,
                    reason: "Stock inicial al crear producto",
                    createdByUserId: currentUser.UserId);
                unitOfWork.Add(movement);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return product.Id;
    }

    private static string GenerateSlug(string name) =>
        Regex.Replace(
            name.ToLowerInvariant().Trim()
                .Replace(" ", "-")
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u").Replace("ü", "u")
                .Replace("ñ", "n").Replace("ä", "a").Replace("ö", "o"),
            @"[^a-z0-9\-]", "")
        .Trim('-');

    private static string NormalizeSlug(string slug) =>
        Regex.Replace(slug.ToLowerInvariant().Trim(), @"[^a-z0-9\-]", "").Trim('-');
}
