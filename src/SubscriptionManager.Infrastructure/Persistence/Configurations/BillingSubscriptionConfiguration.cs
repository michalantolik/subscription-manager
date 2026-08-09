using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Identity;

namespace SubscriptionManager.Infrastructure.Persistence.Configurations;

public sealed class BillingSubscriptionConfiguration
    : IEntityTypeConfiguration<BillingSubscription>
{
    public void Configure(EntityTypeBuilder<BillingSubscription> builder)
    {
        builder.ToTable("BillingSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Plan)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.BillingInterval)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CurrentPeriodStart)
            .IsRequired();

        builder.Property(x => x.CurrentPeriodEnd)
            .IsRequired();

        builder.Property(x => x.CancelAtPeriodEnd)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<BillingSubscription>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
