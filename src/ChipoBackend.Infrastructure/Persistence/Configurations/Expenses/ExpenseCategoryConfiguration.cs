using ChipoBackend.Domain.Entities.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Expenses;

public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        builder.ToTable("expense_categories", "expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Color)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#6B7280");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CreatedByUserId)
            .HasColumnType("uuid");

        builder.Property(e => e.UpdatedByUserId)
            .HasColumnType("uuid");

        builder.HasIndex(e => e.Name)
            .IsUnique();

        builder.Ignore(e => e.DomainEvents);
    }
}
