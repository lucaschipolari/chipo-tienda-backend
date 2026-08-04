using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Combos;

/// <summary>
/// Combo fijo: un conjunto de productos/variantes que se venden juntos a un
/// precio especial. Al venderse se "expande" en sus ítems para descontar el
/// stock real de cada producto (decant por ml, perfume por unidad).
/// </summary>
public class Combo : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public Money Price { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private readonly List<ComboItem> _items = [];
    public IReadOnlyCollection<ComboItem> Items => _items.AsReadOnly();

    private Combo() { }

    public static Combo Create(string name, string slug, Money price, string? description, string? imageUrl)
    {
        return new Combo
        {
            Name = name,
            Slug = slug,
            Price = price,
            Description = description,
            ImageUrl = imageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Update(string name, string slug, Money price, string? description, string? imageUrl)
    {
        Name = name;
        Slug = slug;
        Price = price;
        Description = description;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, Guid variantId, int quantity)
    {
        _items.Add(ComboItem.Create(Id, productId, variantId, quantity));
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearItems()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
