using ChipoBackend.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Catalog;

public class PricingCalculatorEntryConfiguration : IEntityTypeConfiguration<PricingCalculatorEntry>
{
    public void Configure(EntityTypeBuilder<PricingCalculatorEntry> builder)
    {
        builder.ToTable("pricing_calculator_entries", "catalog");
        builder.HasKey(e => e.ProductId);

        builder.Property(e => e.CostUsdOrigin).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.PercentageCommission).HasColumnType("decimal(7,3)").IsRequired();
        builder.Property(e => e.FixedCommission).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.ShippingCost).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.MarginType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.MarginValue).HasColumnType("decimal(14,2)").IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired();

        // Una entrada por producto; si el producto se borra, su entrada también.
        builder.HasOne<Product>()
            .WithOne()
            .HasForeignKey<PricingCalculatorEntry>(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
