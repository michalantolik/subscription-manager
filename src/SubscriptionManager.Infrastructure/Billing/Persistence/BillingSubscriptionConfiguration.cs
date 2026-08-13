using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Common.Identity;

namespace SubscriptionManager.Infrastructure.Billing.Persistence;

/// <summary>
/// Configures persistence for billing subscriptions.
/// </summary>
public sealed class BillingSubscriptionConfiguration
    : IEntityTypeConfiguration<BillingSubscription>
{
    public void Configure(
        EntityTypeBuilder<BillingSubscription> builder)
    {
        builder.ToTable(
            "BillingSubscriptions");

        builder.HasKey(
            subscription =>
                subscription.Id);

        builder.Property(
                subscription =>
                    subscription.Plan)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(
                subscription =>
                    subscription.BillingInterval)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(
                subscription =>
                    subscription.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(
                subscription =>
                    subscription.ProviderCustomerId)
            .HasMaxLength(255);

        builder.Property(
                subscription =>
                    subscription.ProviderSubscriptionId)
            .HasMaxLength(255);

        builder.Property(
                subscription =>
                    subscription.ProviderPriceId)
            .HasMaxLength(255);

        builder.Property(
                subscription =>
                    subscription.CurrentPeriodStart)
            .IsRequired();

        builder.Property(
                subscription =>
                    subscription.CurrentPeriodEnd)
            .IsRequired();

        builder.Property(
            subscription =>
                subscription.LastProviderEventCreatedAt);

        builder.Property(
                subscription =>
                    subscription.CancelAtPeriodEnd)
            .IsRequired();

        builder.HasIndex(
                subscription =>
                    subscription.UserId)
            .IsUnique();

        builder.HasIndex(
            subscription =>
                subscription.ProviderCustomerId);

        builder.HasIndex(
                subscription =>
                    subscription.ProviderSubscriptionId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<BillingSubscription>(
                subscription =>
                    subscription.UserId)
            .OnDelete(
                DeleteBehavior.Cascade);
    }
}
