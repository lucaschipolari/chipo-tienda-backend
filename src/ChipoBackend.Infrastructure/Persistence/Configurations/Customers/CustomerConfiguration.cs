using ChipoBackend.Domain.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers", "crm");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.Property(c => c.PhoneNumber).HasMaxLength(20);

        builder.Property(c => c.DocumentNumber).HasMaxLength(20).IsRequired();
        builder.Property(c => c.DocumentType)
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.DocumentNumber).IsUnique();

        builder.Property(c => c.Street).HasMaxLength(300);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Province).HasMaxLength(100);
        builder.Property(c => c.PostalCode).HasMaxLength(20);

        builder.Property(c => c.CustomerType)
            .HasConversion<string>().HasMaxLength(20);

        builder.Property(c => c.Notes).HasMaxLength(1000);

        builder.HasMany(c => c.Addresses).WithOne().HasForeignKey(a => a.CustomerId);
        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.FullName);
    }
}

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("customer_addresses", "crm");
        builder.HasKey(a => a.Id);
        builder.OwnsOne(a => a.Address, addr =>
        {
            addr.Property(x => x.Street).HasColumnName("street");
            addr.Property(x => x.City).HasColumnName("city").HasMaxLength(100);
            addr.Property(x => x.State).HasColumnName("state").HasMaxLength(100);
            addr.Property(x => x.PostalCode).HasColumnName("postal_code").HasMaxLength(20);
            addr.Property(x => x.Country).HasColumnName("country").HasMaxLength(100);
        });
        builder.Ignore(a => a.DomainEvents);
    }
}
