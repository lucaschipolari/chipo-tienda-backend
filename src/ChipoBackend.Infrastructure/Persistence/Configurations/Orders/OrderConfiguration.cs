using ChipoBackend.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Orders;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", "orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);

        builder.OwnsOne(o => o.ShippingAddress, addr =>
        {
            addr.Property(a => a.Street).HasColumnName("shipping_street");
            addr.Property(a => a.City).HasColumnName("shipping_city").HasMaxLength(100);
            addr.Property(a => a.State).HasColumnName("shipping_state").HasMaxLength(100);
            addr.Property(a => a.PostalCode).HasColumnName("shipping_postal").HasMaxLength(20);
            addr.Property(a => a.Country).HasColumnName("shipping_country").HasMaxLength(100);
        });

        builder.OwnsOne(o => o.BillingAddress, addr =>
        {
            addr.Property(a => a.Street).HasColumnName("billing_street");
            addr.Property(a => a.City).HasColumnName("billing_city").HasMaxLength(100);
            addr.Property(a => a.State).HasColumnName("billing_state").HasMaxLength(100);
            addr.Property(a => a.PostalCode).HasColumnName("billing_postal").HasMaxLength(20);
            addr.Property(a => a.Country).HasColumnName("billing_country").HasMaxLength(100);
        });

        ConfigureMoney(builder, o => o.Subtotal, "subtotal");
        ConfigureMoney(builder, o => o.DiscountAmount, "discount_amount");
        ConfigureMoney(builder, o => o.ShippingCost, "shipping_cost");
        ConfigureMoney(builder, o => o.TaxAmount, "tax_amount");
        ConfigureMoney(builder, o => o.Total, "total");

        builder.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId);
        builder.HasMany(o => o.StatusHistory).WithOne().HasForeignKey(h => h.OrderId);
        builder.HasMany(o => o.Payments).WithOne().HasForeignKey(p => p.OrderId);

        builder.Ignore(o => o.DomainEvents);
    }

    private static void ConfigureMoney<TEntity>(EntityTypeBuilder<TEntity> builder,
        System.Linq.Expressions.Expression<Func<TEntity, ChipoBackend.Domain.ValueObjects.Money>> nav,
        string columnPrefix) where TEntity : class
    {
        builder.OwnsOne(nav, m =>
        {
            m.Property(x => x.Amount).HasColumnName(columnPrefix).HasColumnType("decimal(12,2)");
            m.Property(x => x.Currency).HasColumnName($"{columnPrefix}_currency").HasMaxLength(3).HasDefaultValue("PEN");
        });
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items", "orders");
        builder.HasKey(i => i.Id);
        builder.OwnsOne(i => i.UnitPrice, m => { m.Property(x => x.Amount).HasColumnName("unit_price").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("unit_price_currency").HasMaxLength(3); });
        builder.OwnsOne(i => i.Discount, m => { m.Property(x => x.Amount).HasColumnName("discount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("discount_currency").HasMaxLength(3); });
        builder.OwnsOne(i => i.Total, m => { m.Property(x => x.Amount).HasColumnName("total").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("total_currency").HasMaxLength(3); });
        builder.Ignore(i => i.DomainEvents);
    }
}

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history", "orders");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.FromStatus).HasConversion<string?>().HasMaxLength(30);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(30);
        builder.Ignore(h => h.DomainEvents);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", "orders");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.OwnsOne(p => p.Amount, m => { m.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)"); m.Property(x => x.Currency).HasColumnName("amount_currency").HasMaxLength(3); });
        builder.Ignore(p => p.DomainEvents);
    }
}
