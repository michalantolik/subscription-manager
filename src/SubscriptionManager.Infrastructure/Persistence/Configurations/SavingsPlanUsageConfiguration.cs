using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionManager.Domain.SavingsPlans;

namespace SubscriptionManager.Infrastructure.Persistence.Configurations;

internal sealed class SavingsPlanUsageConfiguration
    : IEntityTypeConfiguration<SavingsPlanUsage>
{
    public void Configure(
        EntityTypeBuilder<SavingsPlanUsage> builder)
    {
        builder.ToTable("SavingsPlanUsages");

        builder.HasKey(x => new
        {
            x.UserId,
            x.UsageDateUtc
        });

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.UsageDateUtc)
            .IsRequired();

        builder.Property(x => x.RequestCount)
            .IsRequired();
    }
}
