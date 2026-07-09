using ChipoBackend.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Finance;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses", "finance");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Description).IsRequired();
        builder.OwnsOne(e => e.Amount, m =>
        {
            m.Property(x => x.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
            m.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("ARS");
        });
        builder.Ignore(e => e.DomainEvents);
    }
}
