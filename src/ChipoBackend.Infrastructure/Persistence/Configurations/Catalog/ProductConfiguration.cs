using ChipoBackend.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Catalog;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(255).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Sku).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Tags).HasColumnType("text[]");

        // Perfil olfativo
        builder.Property(p => p.TopNotes).HasColumnType("text[]");
        builder.Property(p => p.HeartNotes).HasColumnType("text[]");
        builder.Property(p => p.BaseNotes).HasColumnType("text[]");
        builder.Property(p => p.Intensity);
        builder.Property(p => p.Longevity).HasMaxLength(50);
        builder.Property(p => p.Seasons).HasColumnType("text[]");
        builder.Property(p => p.Occasions).HasColumnType("text[]");

        builder.OwnsOne(p => p.BasePrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("base_price").HasColumnType("decimal(12,2)").IsRequired();
            money.Property(m => m.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("ARS").ValueGeneratedNever();
        });

        builder.OwnsOne(p => p.CompareAtPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("compare_at_price").HasColumnType("decimal(12,2)");
            money.Property(m => m.Currency).HasColumnName("compare_at_currency").HasMaxLength(3);
        });

        builder.HasMany(p => p.Variants).WithOne().HasForeignKey(v => v.ProductId);
        builder.HasMany(p => p.Images).WithOne().HasForeignKey(i => i.ProductId);

        builder.HasMany(p => p.RelatedProducts)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId);

        builder.Ignore(p => p.DomainEvents);
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants", "catalog");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Sku).HasMaxLength(100).IsRequired();
        builder.HasIndex(v => v.Sku).IsUnique();
        builder.Property(v => v.Attributes).HasColumnType("jsonb");

        builder.OwnsOne(v => v.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("price").HasColumnType("decimal(12,2)");
            money.Property(m => m.Currency).HasColumnName("price_currency").HasMaxLength(3);
        });

        builder.OwnsOne(v => v.CompareAtPrice, money =>
        {
            money.Property(m => m.Amount).HasColumnName("compare_at_price").HasColumnType("decimal(12,2)");
            money.Property(m => m.Currency).HasColumnName("compare_at_currency").HasMaxLength(3);
        });

        builder.Ignore(v => v.DomainEvents);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", "catalog");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(150).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.DomainEvents);
    }
}

public class ProductRelationConfiguration : IEntityTypeConfiguration<ProductRelation>
{
    public void Configure(EntityTypeBuilder<ProductRelation> builder)
    {
        builder.ToTable("product_relations", "catalog");
        builder.HasKey(r => new { r.ProductId, r.RelatedProductId });
        builder.HasOne(r => r.RelatedProduct).WithMany().HasForeignKey(r => r.RelatedProductId);
    }
}
