namespace ChipoBackend.Domain.Entities.Catalog;

public class ProductRelation
{
    public Guid ProductId { get; set; }
    public Guid RelatedProductId { get; set; }
    public string RelationType { get; set; } = "related";
    public Product Product { get; set; } = null!;
    public Product RelatedProduct { get; set; } = null!;
}
