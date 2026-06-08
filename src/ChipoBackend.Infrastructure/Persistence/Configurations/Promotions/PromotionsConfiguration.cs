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
        builder.Property(d => d.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.AppliesTo).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.TargetIds).HasColumnType("uuid[]");
        builder.OwnsOne(d => d.MinOrderAmount, m => { m.Property(x => x.Amount).HasColumnName("min_order_amount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("min_order_currency").HasMaxLength(3); });
        builder.OwnsOne(d => d.MaxDiscountAmount, m => { m.Property(x => x.Amount).HasColumnName("max_discount_amount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("max_discount_currency").HasMaxLength(3); });
        builder.Ignore(d => d.DomainEvents);
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
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
        builder.OwnsOne(c => c.MinOrderAmount, m => { m.Property(x => x.Amount).HasColumnName("min_order_amount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("min_order_currency").HasMaxLength(3); });
        builder.OwnsOne(c => c.MaxDiscountAmount, m => { m.Property(x => x.Amount).HasColumnName("max_discount_amount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("max_discount_currency").HasMaxLength(3); });
        builder.HasMany(c => c.Usages).WithOne().HasForeignKey(u => u.CouponId);
        builder.Ignore(c => c.DomainEvents);
    }
}

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("coupon_usages", "promotions");
        builder.HasKey(u => u.Id);
        builder.Ignore(u => u.DomainEvents);
    }
}
