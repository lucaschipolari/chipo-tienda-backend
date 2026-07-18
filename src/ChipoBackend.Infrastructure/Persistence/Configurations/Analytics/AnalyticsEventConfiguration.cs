using ChipoBackend.Domain.Entities.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChipoBackend.Infrastructure.Persistence.Configurations.Analytics;

public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.ToTable("analytics_events", "analytics");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type).HasConversion<int>();
        builder.Property(e => e.SearchTerm).HasMaxLength(120);
        builder.Property(e => e.SessionId).HasMaxLength(64);

        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.Type, e.CreatedAt });
        builder.HasIndex(e => new { e.ProductId, e.Type });
    }
}
