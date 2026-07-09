using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Categories.DTOs;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;

public class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository,
    IProductRepository productRepository
) : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Categoría '{request.Id}' no encontrada.");

        var (_, productCount) = await productRepository.GetPagedAsync(1, 1, request.Id, ct: ct);

        return new CategoryDto(
            Id: category.Id,
            ParentCategoryId: category.ParentCategoryId,
            Name: category.Name,
            Slug: category.Slug,
            Description: category.Description,
            ImageUrl: category.ImageUrl,
            DisplayOrder: category.DisplayOrder,
            IsActive: category.IsActive,
            ProductCount: productCount,
            SubCategories: [],
            CreatedAt: category.CreatedAt,
            UpdatedAt: category.UpdatedAt
        );
    }
}
