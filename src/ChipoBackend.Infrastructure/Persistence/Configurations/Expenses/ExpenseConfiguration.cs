using ChipoBackend.Domain.Entities.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Expenses;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses", "expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CategoryId)
            .IsRequired()
            .HasColumnType("uuid");

        builder.Property(e => e.Date)
            .IsRequired()
            .HasColumnType("date");

        builder.OwnsOne(e => e.Amount, m =>
        {
            m.Property(x => x.Amount)
                .IsRequired()
                .HasColumnName("Amount_Amount")
                .HasColumnType("decimal(18,4)");

            m.Property(x => x.Currency)
                .IsRequired()
                .HasColumnName("Amount_Currency")
                .HasMaxLength(3)
                .HasDefaultValue("ARS");
        });

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Observations)
            .HasColumnType("text");

        builder.Property(e => e.ReceiptUrl)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ExpenseStatus.Pending);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CreatedByUserId)
            .HasColumnType("uuid");

        builder.Property(e => e.UpdatedByUserId)
            .HasColumnType("uuid");

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.Date);
        builder.HasIndex(e => e.Status);

        builder.Ignore(e => e.DomainEvents);
    }
}
