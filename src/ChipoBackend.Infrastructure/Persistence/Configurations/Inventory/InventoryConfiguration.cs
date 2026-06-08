using ChipoBackend.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Inventory;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements", "inventory");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MovementType).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(m => new { m.VariantId, m.CreatedAt });
        builder.Ignore(m => m.DomainEvents);
    }
}

public class LostSaleConfiguration : IEntityTypeConfiguration<LostSale>
{
    public void Configure(EntityTypeBuilder<LostSale> builder)
    {
        builder.ToTable("lost_sales", "inventory");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Source).HasConversion<string>().HasMaxLength(30);
        builder.Ignore(l => l.DomainEvents);
    }
}
