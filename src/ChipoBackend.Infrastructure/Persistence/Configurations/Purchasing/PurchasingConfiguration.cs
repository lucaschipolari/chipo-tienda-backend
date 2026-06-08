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
        builder.OwnsOne(s => s.Address, addr =>
        {
            addr.Property(x => x.Street).HasColumnName("street");
            addr.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
            addr.Property(x => x.State).HasColumnName("state").HasMaxLength(100);
            addr.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(20);
            addr.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);
        });
        builder.Ignore(s => s.DomainEvents);
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
