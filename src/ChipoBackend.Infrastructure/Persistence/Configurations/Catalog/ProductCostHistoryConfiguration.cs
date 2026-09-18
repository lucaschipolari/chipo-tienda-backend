using ChipoBackend.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Catalog;

public class ProductCostHistoryConfiguration : IEntityTypeConfiguration<ProductCostHistory>
{
    public void Configure(EntityTypeBuilder<ProductCostHistory> builder)
    {
        builder.ToTable("product_cost_history", "catalog");
        builder.HasKey(h => h.Id);

        builder.OwnsOne(h => h.UnitCost, money =>
        {
            money.Property(m => m.Amount).HasColumnName("unit_cost").HasColumnType("decimal(12,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("unit_cost_currency").HasMaxLength(3).IsRequired();
        });

        builder.Property(h => h.Source).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(h => h.RecordedAt).IsRequired();

        builder.HasIndex(h => h.VariantId);
        builder.HasIndex(h => h.ProductId);
        builder.HasIndex(h => new { h.VariantId, h.RecordedAt });
    }
}
