using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Catalog;

public class Category : AuditableEntity
{
    public Guid? ParentCategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Category? ParentCategory { get; private set; }
    private readonly List<Category> _subCategories = [];
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private Category() { }

    public static Category Create(string name, string slug, Guid? parentId = null, string? description = null, string? imageUrl = null, int displayOrder = 0)
    {
        return new Category
        {
            Name = name,
            Slug = slug,
            ParentCategoryId = parentId,
            Description = description,
            ImageUrl = imageUrl,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string slug, Guid? parentId, string? description, string? imageUrl, int displayOrder)
    {
        Name = name;
        Slug = slug;
        ParentCategoryId = parentId;
        Description = description;
        ImageUrl = imageUrl;
        DisplayOrder = displayOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
}
