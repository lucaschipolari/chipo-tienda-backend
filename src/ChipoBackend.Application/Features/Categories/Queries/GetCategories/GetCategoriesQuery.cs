using ChipoBackend.Application.Features.Categories.DTOs;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Categories.Queries.GetCategories;

public record GetCategoriesQuery(bool IncludeInactive = false) : IRequest<List<CategoryDto>>;

public class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    IProductRepository productRepository
) : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var all = await categoryRepository.GetTreeAsync(ct);
        var filtered = request.IncludeInactive ? all : all.Where(c => c.IsActive).ToList();

        // Contar productos por categoría (solo roots + subcategorías de primer nivel)
        var productCounts = new Dictionary<Guid, int>();
        foreach (var cat in all)
        {
            var (_, count) = await productRepository.GetPagedAsync(1, 1, cat.Id, ct: ct);
            productCounts[cat.Id] = count;
        }

        // Construir árbol — GetTreeAsync ya devuelve las categorías con subcategorías anidadas
        var roots = filtered.Where(c => c.ParentCategoryId == null).ToList();
        return roots.Select(c => MapToDto(c, productCounts, request.IncludeInactive)).ToList();
    }

    private static CategoryDto MapToDto(Category c, Dictionary<Guid, int> counts, bool includeInactive)
    {
        var subs = c.SubCategories
            .Where(s => includeInactive || s.IsActive)
            .Select(s => MapToDto(s, counts, includeInactive))
            .OrderBy(s => s.DisplayOrder)
            .ToList();

        return new CategoryDto(
            Id: c.Id,
            ParentCategoryId: c.ParentCategoryId,
            Name: c.Name,
            Slug: c.Slug,
            Description: c.Description,
            ImageUrl: c.ImageUrl,
            DisplayOrder: c.DisplayOrder,
            IsActive: c.IsActive,
            ProductCount: counts.GetValueOrDefault(c.Id),
            SubCategories: subs,
            CreatedAt: c.CreatedAt,
            UpdatedAt: c.UpdatedAt
        );
    }
}
