using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Infrastructure.Subscriptions;

/// <summary>
/// Configures persistence for subscriptions.
/// </summary>
internal sealed class SubscriptionConfiguration
    : IEntityTypeConfiguration<Subscription>
{
    public void Configure(
        EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.HasIndex(x => x.OwnerId);

        builder.Property(x => x.DigitalServiceId);

        builder.HasOne<DigitalService>()
            .WithMany()
            .HasForeignKey(x => x.DigitalServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(Subscription.MaxNameLength);

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.CustomCategoryName)
            .HasMaxLength(200);

        builder.Property(x => x.IconKey)
            .HasMaxLength(100);

        builder.Property(x => x.ManagementUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.BillingPeriod)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate);

        builder.Ignore(x => x.IsActive);

        builder.Ignore(x => x.MonthlyEquivalentAmount);

        builder.Ignore(x => x.YearlyEquivalentAmount);
    }
}
