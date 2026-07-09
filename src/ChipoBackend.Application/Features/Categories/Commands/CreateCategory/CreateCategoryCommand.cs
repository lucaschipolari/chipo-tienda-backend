using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using System.Text.RegularExpressions;

namespace ChipoBackend.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Slug,
    Guid? ParentCategoryId,
    string? Description,
    string? ImageUrl,
    int DisplayOrder
) : IRequest<Guid>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("El nombre es requerido (máx 150 caracteres).");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ImageUrl).MaximumLength(500).When(x => x.ImageUrl != null);
    }
}

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : request.Slug.ToLowerInvariant().Trim();

        if (await categoryRepository.SlugExistsAsync(slug, ct: ct))
            throw new ConflictException($"El slug '{slug}' ya está en uso.");

        if (request.ParentCategoryId.HasValue)
        {
            _ = await categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException($"Categoría padre '{request.ParentCategoryId}' no encontrada.");
        }

        var category = Category.Create(request.Name, slug, request.ParentCategoryId,
            request.Description, request.ImageUrl, request.DisplayOrder);

        unitOfWork.Add(category);
        await unitOfWork.SaveChangesAsync(ct);
        return category.Id;
    }

    private static string GenerateSlug(string name) =>
        Regex.Replace(
            name.ToLowerInvariant().Trim()
                .Replace(" ", "-")
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n"),
            @"[^a-z0-9\-]", "").Trim('-');
}
