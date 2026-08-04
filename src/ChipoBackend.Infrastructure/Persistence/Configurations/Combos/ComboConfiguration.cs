using ChipoBackend.Domain.Entities.Combos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Combos;

public class ComboConfiguration : IEntityTypeConfiguration<Combo>
{
    public void Configure(EntityTypeBuilder<Combo> builder)
    {
        builder.ToTable("combos", "catalog");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(220).IsRequired();
        builder.Property(c => c.Description).HasColumnType("text");
        builder.Property(c => c.ImageUrl).HasMaxLength(1000);
        builder.OwnsOne(c => c.Price, m =>
        {
            m.Property(x => x.Amount).HasColumnName("price").HasColumnType("decimal(12,2)");
            m.Property(x => x.Currency).HasColumnName("price_currency").HasMaxLength(3);
        });
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.HasMany(c => c.Items).WithOne().HasForeignKey(i => i.ComboId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ComboItemConfiguration : IEntityTypeConfiguration<ComboItem>
{
    public void Configure(EntityTypeBuilder<ComboItem> builder)
    {
        builder.ToTable("combo_items", "catalog");
        builder.HasKey(i => i.Id);
    }
}
