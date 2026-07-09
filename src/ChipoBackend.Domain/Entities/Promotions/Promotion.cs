using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Promotions;

public enum PromotionType { Product, Category, BuyXGetY, MinAmount, Combo, Flash, HappyHour }

public class Promotion : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PromotionType Type { get; private set; }
    public string? Badge { get; private set; }

    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public TimeOnly? ActiveFrom { get; private set; }
    public TimeOnly? ActiveUntil { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool IsStackable { get; private set; } = true;
    public int Priority { get; private set; } = 0;

    public DiscountType DiscountType { get; private set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; private set; }
    public string Currency { get; private set; } = "ARS";

    // BuyXGetY
    public int? BuyQuantity { get; private set; }
    public int? GetQuantity { get; private set; }

    // MinAmount
    public decimal? MinOrderAmount { get; private set; }

    // Combo price override
    public decimal? ComboPrice { get; private set; }

    private readonly List<PromotionProduct> _products = [];
    private readonly List<PromotionCategory> _categories = [];
    public IReadOnlyCollection<PromotionProduct> Products => _products.AsReadOnly();
    public IReadOnlyCollection<PromotionCategory> Categories => _categories.AsReadOnly();

    public bool IsCurrentlyValid
    {
        get
        {
            if (!IsActive) return false;
            var now = DateTime.UtcNow;
            if (StartsAt > now) return false;
            if (EndsAt.HasValue && EndsAt < now) return false;
            if (ActiveFrom.HasValue && ActiveUntil.HasValue)
            {
                var timeNow = TimeOnly.FromDateTime(now);
                if (timeNow < ActiveFrom || timeNow > ActiveUntil) return false;
            }
            return true;
        }
    }

    private Promotion() { }

    public static Promotion Create(string name, PromotionType type, DiscountType discountType,
        decimal discountValue, DateTime startsAt, DateTime? endsAt = null,
        string? description = null, string? badge = null, string currency = "ARS",
        bool isStackable = true, int priority = 0,
        TimeOnly? activeFrom = null, TimeOnly? activeUntil = null,
        int? buyQuantity = null, int? getQuantity = null,
        decimal? minOrderAmount = null, decimal? comboPrice = null,
        Guid? createdBy = null)
    {
        return new Promotion
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            Badge = badge?.Trim(),
            StartsAt = startsAt,
            EndsAt = endsAt,
            ActiveFrom = activeFrom,
            ActiveUntil = activeUntil,
            IsActive = true,
            IsStackable = isStackable,
            Priority = priority,
            DiscountType = discountType,
            DiscountValue = discountValue,
            Currency = currency,
            BuyQuantity = buyQuantity,
            GetQuantity = getQuantity,
            MinOrderAmount = minOrderAmount,
            ComboPrice = comboPrice,
            CreatedByUserId = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void Update(string name, PromotionType type, DiscountType discountType,
        decimal discountValue, DateTime startsAt, DateTime? endsAt,
        string? description, string? badge, string currency,
        bool isStackable, int priority,
        TimeOnly? activeFrom, TimeOnly? activeUntil,
        int? buyQuantity, int? getQuantity,
        decimal? minOrderAmount, decimal? comboPrice, Guid? updatedBy = null)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Type = type;
        Badge = badge?.Trim();
        StartsAt = startsAt;
        EndsAt = endsAt;
        ActiveFrom = activeFrom;
        ActiveUntil = activeUntil;
        IsStackable = isStackable;
        Priority = priority;
        DiscountType = discountType;
        DiscountValue = discountValue;
        Currency = currency;
        BuyQuantity = buyQuantity;
        GetQuantity = getQuantity;
        MinOrderAmount = minOrderAmount;
        ComboPrice = comboPrice;
        UpdatedByUserId = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Toggle() { IsActive = !IsActive; UpdatedAt = DateTime.UtcNow; }

    public void SetProducts(List<Guid> productIds)
    {
        _products.Clear();
        foreach (var id in productIds)
            _products.Add(new PromotionProduct { PromotionId = Id, ProductId = id });
    }

    public void SetCategories(List<Guid> categoryIds)
    {
        _categories.Clear();
        foreach (var id in categoryIds)
            _categories.Add(new PromotionCategory { PromotionId = Id, CategoryId = id });
    }
}
