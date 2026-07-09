using ChipoBackend.Domain.Entities.Purchasing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Purchasing;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers", "purchasing");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.TradeName).HasMaxLength(200);
        builder.Property(s => s.ContactName).HasMaxLength(200);
        builder.Property(s => s.Email).HasMaxLength(255);
        builder.Property(s => s.Phone).HasMaxLength(50);
        builder.Property(s => s.TaxId).HasMaxLength(20);
        builder.Property(s => s.Website).HasMaxLength(500);
        builder.Property(s => s.City).HasMaxLength(100);
        builder.Property(s => s.Province).HasMaxLength(100);
        builder.Property(s => s.Country).HasMaxLength(100);
        builder.Property(s => s.PaymentTerms).HasMaxLength(200);
        builder.Property(s => s.Notes).HasMaxLength(1000);

        builder.OwnsOne(s => s.Address, addr =>
        {
            addr.Property(x => x.Street).HasColumnName("street");
            addr.Property(x => x.City).HasColumnName("address_city").HasMaxLength(100);
            addr.Property(x => x.State).HasColumnName("state").HasMaxLength(100);
            addr.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(20);
            addr.Property(x => x.Country).HasColumnName("address_country").HasMaxLength(100);
        });

        builder.HasMany(s => s.Contacts)
            .WithOne()
            .HasForeignKey(c => c.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Products)
            .WithOne()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);
    }
}

public class SupplierContactConfiguration : IEntityTypeConfiguration<SupplierContact>
{
    public void Configure(EntityTypeBuilder<SupplierContact> builder)
    {
        builder.ToTable("supplier_contacts", "purchasing");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.JobTitle).HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.Property(c => c.Phone).HasMaxLength(50);

        builder.Ignore(c => c.DomainEvents);
    }
}

public class SupplierProductConfiguration : IEntityTypeConfiguration<SupplierProduct>
{
    public void Configure(EntityTypeBuilder<SupplierProduct> builder)
    {
        builder.ToTable("supplier_products", "purchasing");
        builder.HasKey(sp => sp.Id);

        builder.HasIndex(sp => new { sp.SupplierId, sp.ProductId }).IsUnique();

        builder.Property(sp => sp.SupplierProductCode).HasMaxLength(100);
        builder.Property(sp => sp.PurchasePrice).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(sp => sp.Currency).HasMaxLength(3).IsRequired();
        builder.Property(sp => sp.LeadTimeDays).IsRequired();

        builder.Ignore(sp => sp.DomainEvents);
    }
}

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders", "purchasing");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.PurchaseNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => p.PurchaseNumber).IsUnique();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Notes).HasMaxLength(1000);
        builder.OwnsOne(p => p.Subtotal, m => { m.Property(x => x.Amount).HasColumnName("subtotal").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("subtotal_currency").HasMaxLength(3); });
        builder.OwnsOne(p => p.TaxAmount, m => { m.Property(x => x.Amount).HasColumnName("tax_amount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("tax_currency").HasMaxLength(3); });
        builder.OwnsOne(p => p.Total, m => { m.Property(x => x.Amount).HasColumnName("total").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("total_currency").HasMaxLength(3); });
        builder.HasMany(p => p.Items).WithOne().HasForeignKey(i => i.PurchaseOrderId);
        builder.Ignore(p => p.DomainEvents);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("purchase_order_items", "purchasing");
        builder.HasKey(i => i.Id);
        builder.OwnsOne(i => i.UnitCost, m => { m.Property(x => x.Amount).HasColumnName("unit_cost").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("unit_cost_currency").HasMaxLength(3); });
        builder.OwnsOne(i => i.Total, m => { m.Property(x => x.Amount).HasColumnName("total").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("total_currency").HasMaxLength(3); });
        builder.Ignore(i => i.DomainEvents);
    }
}
