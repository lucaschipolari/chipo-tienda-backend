using ChipoBackend.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Promotions;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("discounts", "promotions");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.AppliesTo).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Currency).HasMaxLength(3).HasDefaultValue("ARS");
        builder.Property(d => d.TargetIds).HasColumnType("uuid[]");
        builder.Property(d => d.IsDeleted).HasDefaultValue(false);
        builder.Property(d => d.IsStackable).HasDefaultValue(true);
        builder.Property(d => d.Priority).HasDefaultValue(0);
        builder.OwnsOne(d => d.MinOrderAmount, m => {
            m.Property(x => x.Amount).HasColumnName("min_order_amount").HasColumnType("decimal(12,2)");
            m.Property(x => x.Currency).HasColumnName("min_order_currency").HasMaxLength(3);
        });
        builder.OwnsOne(d => d.MaxDiscountAmount, m => {
            m.Property(x => x.Amount).HasColumnName("max_discount_amount").HasColumnType("decimal(12,2)");
            m.Property(x => x.Currency).HasColumnName("max_discount_currency").HasMaxLength(3);
        });
        builder.Ignore(d => d.DomainEvents);
    }
}

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions", "promotions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Badge).HasMaxLength(50);
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.DiscountType).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("ARS");
        builder.Property(p => p.MinOrderAmount).HasColumnType("decimal(12,2)");
        builder.Property(p => p.ComboPrice).HasColumnType("decimal(12,2)");
        builder.Property(p => p.DiscountValue).HasColumnType("decimal(8,4)");
        builder.Property(p => p.IsStackable).HasDefaultValue(true);
        builder.Property(p => p.Priority).HasDefaultValue(0);
        builder.HasMany(p => p.Products).WithOne(pp => pp.Promotion).HasForeignKey(pp => pp.PromotionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Categories).WithOne(pc => pc.Promotion).HasForeignKey(pc => pc.PromotionId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(p => p.DomainEvents);
    }
}

public class PromotionProductConfiguration : IEntityTypeConfiguration<PromotionProduct>
{
    public void Configure(EntityTypeBuilder<PromotionProduct> builder)
    {
        builder.ToTable("promotion_products", "promotions");
        builder.HasKey(pp => new { pp.PromotionId, pp.ProductId });
    }
}

public class PromotionCategoryConfiguration : IEntityTypeConfiguration<PromotionCategory>
{
    public void Configure(EntityTypeBuilder<PromotionCategory> builder)
    {
        builder.ToTable("promotion_categories", "promotions");
        builder.HasKey(pc => new { pc.PromotionId, pc.CategoryId });
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons", "promotions");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.Currency).HasMaxLength(3).HasDefaultValue("ARS");
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.Property(c => c.IsStackable).HasDefaultValue(false);
        builder.OwnsOne(c => c.MinOrderAmount, m => {
            m.Property(x => x.Amount).HasColumnName("min_order_amount").HasColumnType("decimal(12,2)");
            m.Property(x => x.Currency).HasColumnName("min_order_currency").HasMaxLength(3);
        });
        builder.OwnsOne(c => c.MaxDiscountAmount, m => {
            m.Property(x => x.Amount).HasColumnName("max_discount_amount").HasColumnType("decimal(12,2)");
            m.Property(x => x.Currency).HasColumnName("max_discount_currency").HasMaxLength(3);
        });
        builder.HasMany(c => c.Usages).WithOne().HasForeignKey(u => u.CouponId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Restrictions).WithOne().HasForeignKey(r => r.CouponId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(c => c.DomainEvents);
    }
}

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("coupon_usages", "promotions");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.DiscountAmount).HasColumnType("decimal(12,2)");
        builder.Ignore(u => u.DomainEvents);
    }
}

public class CouponRestrictionConfiguration : IEntityTypeConfiguration<CouponRestriction>
{
    public void Configure(EntityTypeBuilder<CouponRestriction> builder)
    {
        builder.ToTable("coupon_restrictions", "promotions");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
        builder.Ignore(r => r.DomainEvents);
    }
}
