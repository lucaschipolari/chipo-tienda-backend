using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Exceptions;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Catalog;

public class Product : AuditableEntity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Sku { get; private set; } = null!;
    public Money BasePrice { get; private set; } = null!;
    public Money? CompareAtPrice { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Draft;
    public bool IsFeatured { get; private set; }
    public List<string> Tags { get; private set; } = [];

    // ── Perfil olfativo (opcional) ──────────────────────────────────────────
    public List<string> TopNotes { get; private set; } = [];      // notas de salida
    public List<string> HeartNotes { get; private set; } = [];    // notas de corazón
    public List<string> BaseNotes { get; private set; } = [];     // notas de fondo
    public int? Intensity { get; private set; }                   // 1–5
    public string? Longevity { get; private set; }                // ej "6-8 horas"
    public List<string> Seasons { get; private set; } = [];       // Primavera, Verano, Otoño, Invierno
    public List<string> Occasions { get; private set; } = [];     // Día, Noche, Formal, Casual

    public Category? Category { get; private set; }

    private readonly List<ProductVariant> _variants = [];
    public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

    private readonly List<ProductImage> _images = [];
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<ProductRelation> _relatedProducts = [];
    public IReadOnlyCollection<ProductRelation> RelatedProducts => _relatedProducts.AsReadOnly();

    private Product() { }

    public static Product Create(Guid categoryId, string name, string slug, string sku, Money basePrice, string? description = null)
    {
        return new Product
        {
            CategoryId = categoryId,
            Name = name,
            Slug = slug,
            Sku = sku,
            BasePrice = basePrice,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string slug, Guid categoryId, string? description, Money basePrice, Money? compareAtPrice, bool isFeatured, List<string> tags)
    {
        Name = name;
        Slug = slug;
        CategoryId = categoryId;
        Description = description;
        BasePrice = basePrice;
        CompareAtPrice = compareAtPrice;
        IsFeatured = isFeatured;
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Setea el perfil olfativo. Listas nulas se tratan como vacías.</summary>
    public void SetOlfactoryProfile(
        List<string>? topNotes, List<string>? heartNotes, List<string>? baseNotes,
        int? intensity, string? longevity,
        List<string>? seasons, List<string>? occasions)
    {
        TopNotes = topNotes ?? [];
        HeartNotes = heartNotes ?? [];
        BaseNotes = baseNotes ?? [];
        Intensity = intensity is >= 1 and <= 5 ? intensity : null;
        Longevity = string.IsNullOrWhiteSpace(longevity) ? null : longevity.Trim();
        Seasons = seasons ?? [];
        Occasions = occasions ?? [];
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (!_variants.Any())
            throw new BusinessRuleException("ProductNoVariants", "Un producto debe tener al menos una variante para publicarse.");
        Status = ProductStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDraft()
    {
        Status = ProductStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public ProductVariant AddVariant(string sku, Dictionary<string, string> attributes, int initialStock = 0, Money? price = null, int minStockThreshold = 5, Money? compareAtPrice = null)
    {
        if (_variants.Any(v => v.Sku == sku))
            throw new BusinessRuleException("DuplicateVariantSku", $"El SKU '{sku}' ya existe en este producto.");

        var variant = ProductVariant.Create(Id, sku, attributes, initialStock, price, minStockThreshold, compareAtPrice);
        _variants.Add(variant);
        UpdatedAt = DateTime.UtcNow;
        return variant;
    }

    public void AddImage(string url, string? altText = null, int displayOrder = 0)
    {
        _images.Add(ProductImage.Create(Id, url, altText, displayOrder));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new DomainException($"Imagen {imageId} no encontrada.");
        _images.Remove(image);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRelatedProduct(Guid relatedProductId, string relationType = "related")
    {
        if (_relatedProducts.Any(r => r.RelatedProductId == relatedProductId)) return;
        _relatedProducts.Add(new ProductRelation { ProductId = Id, RelatedProductId = relatedProductId, RelationType = relationType });
    }
}

public enum ProductStatus { Draft, Published, Discontinued }
