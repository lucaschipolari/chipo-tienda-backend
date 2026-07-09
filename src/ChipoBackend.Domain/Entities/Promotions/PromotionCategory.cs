namespace ChipoBackend.Domain.Entities.Promotions;

public class PromotionCategory
{
    public Guid PromotionId { get; set; }
    public Guid CategoryId { get; set; }
    public Promotion? Promotion { get; set; }
}
