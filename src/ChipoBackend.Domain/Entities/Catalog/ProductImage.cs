using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Catalog;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = null!;
    public string? AltText { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(Guid productId, string url, string? altText, int displayOrder)
    {
        return new ProductImage { ProductId = productId, Url = url, AltText = altText, DisplayOrder = displayOrder };
    }
}
