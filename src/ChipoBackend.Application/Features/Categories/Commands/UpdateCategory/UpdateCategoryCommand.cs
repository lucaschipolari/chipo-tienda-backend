using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentCategoryId,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive
) : IRequest;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200)
            .Matches(@"^[a-z0-9\-]+$").WithMessage("El slug solo puede contener letras minúsculas, números y guiones.");
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Categoría '{request.Id}' no encontrada.");

        if (request.Id == request.ParentCategoryId)
            throw new ConflictException("Una categoría no puede ser su propio padre.");

        if (await categoryRepository.SlugExistsAsync(request.Slug, request.Id, ct))
            throw new ConflictException($"El slug '{request.Slug}' ya está en uso.");

        if (request.ParentCategoryId.HasValue)
        {
            _ = await categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException($"Categoría padre '{request.ParentCategoryId}' no encontrada.");
        }

        category.Update(request.Name, request.Slug, request.ParentCategoryId,
            request.Description, request.ImageUrl, request.DisplayOrder);

        if (request.IsActive && !category.IsActive) category.Activate();
        else if (!request.IsActive && category.IsActive) category.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
