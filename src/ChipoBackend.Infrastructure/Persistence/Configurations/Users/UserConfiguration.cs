using ChipoBackend.Domain.Entities.Users;
using ChipoBackend.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");
        builder.HasKey(u => u.Id);

        // Use HasConversion instead of OwnsOne to avoid EF Core 9 generating a separate
        // UPDATE command for the owned entity entry, which caused DbUpdateConcurrencyException
        // (expected 1 row, affected 0 rows) when saving a Modified User entity.
        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Of(value))
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(u => u.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(u => u.Roles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId);

        builder.Ignore(u => u.DomainEvents);
    }
}
