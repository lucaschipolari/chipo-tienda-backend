using ChipoBackend.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Sales;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales", "sales");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SaleNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(s => s.SaleNumber).IsUnique();
        builder.Property(s => s.Channel).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.PaymentMethod).HasMaxLength(30).IsRequired();
        builder.OwnsOne(s => s.Subtotal, m => { m.Property(x => x.Amount).HasColumnName("subtotal").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("subtotal_currency").HasMaxLength(3); });
        builder.OwnsOne(s => s.DiscountAmount, m => { m.Property(x => x.Amount).HasColumnName("discount_amount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("discount_currency").HasMaxLength(3); });
        builder.OwnsOne(s => s.Total, m => { m.Property(x => x.Amount).HasColumnName("total").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("total_currency").HasMaxLength(3); });
        builder.HasMany(s => s.Items).WithOne().HasForeignKey(i => i.SaleId);
        builder.Ignore(s => s.DomainEvents);
    }
}

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items", "sales");
        builder.HasKey(i => i.Id);
        builder.OwnsOne(i => i.UnitPrice, m => { m.Property(x => x.Amount).HasColumnName("unit_price").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("unit_price_currency").HasMaxLength(3); });
        builder.OwnsOne(i => i.Discount, m => { m.Property(x => x.Amount).HasColumnName("discount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("discount_currency").HasMaxLength(3); });
        builder.OwnsOne(i => i.Total, m => { m.Property(x => x.Amount).HasColumnName("total").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("total_currency").HasMaxLength(3); });
        builder.Ignore(i => i.DomainEvents);
    }
}
