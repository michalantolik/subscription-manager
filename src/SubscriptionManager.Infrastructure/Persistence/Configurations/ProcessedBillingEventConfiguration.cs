using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionManager.Infrastructure.Billing;

namespace SubscriptionManager.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedBillingEventConfiguration
    : IEntityTypeConfiguration<ProcessedBillingEvent>
{
    public void Configure(
        EntityTypeBuilder<ProcessedBillingEvent> builder)
    {
        builder.ToTable(
            "ProcessedBillingEvents");

        builder.HasKey(
            billingEvent =>
                billingEvent.ProviderEventId);

        builder.Property(
                billingEvent =>
                    billingEvent.ProviderEventId)
            .HasMaxLength(255)
            .ValueGeneratedNever();

        builder.Property(
                billingEvent =>
                    billingEvent.ProviderEventCreatedAt)
            .IsRequired();

        builder.Property(
                billingEvent =>
                    billingEvent.ProcessedAt)
            .IsRequired();

        builder.HasIndex(
            billingEvent =>
                billingEvent.ProcessedAt);
    }
}
