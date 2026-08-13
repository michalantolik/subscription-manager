using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionManager.Domain.ExchangeRates;

namespace SubscriptionManager.Infrastructure.ExchangeRates;

/// <summary>
/// Configures persistence for exchange rates.
/// </summary>
internal sealed class ExchangeRateConfiguration
    : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(
        EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");

        builder.HasKey(x => x.Currency);

        builder.Property(x => x.Currency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .ValueGeneratedNever();

        builder.Property(x => x.RateToPln)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(x => x.EffectiveDate)
            .IsRequired();

        builder.Property(x => x.LastCheckedAt)
            .IsRequired();
    }
}
