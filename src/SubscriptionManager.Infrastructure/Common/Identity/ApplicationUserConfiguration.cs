using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SubscriptionManager.Infrastructure.Common.Identity;

/// <summary>
/// Configures persistence for application users.
/// </summary>
internal sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(
        EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.Language)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.BaseCurrency)
            .HasConversion<string>()
            .HasMaxLength(3)
            .IsRequired();
    }
}
