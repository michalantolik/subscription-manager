using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Infrastructure.Persistence.Configurations;

internal sealed class DigitalServiceConfiguration
    : IEntityTypeConfiguration<DigitalService>
{
    public void Configure(EntityTypeBuilder<DigitalService> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsPredefined)
            .IsRequired();

        builder.Property(x => x.OwnerId);

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CustomCategoryName)
            .HasMaxLength(200);

        builder.Property(x => x.IconKey)
            .HasMaxLength(100);

        builder.Property(x => x.ManagementUrl)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.SortOrder)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Key)
            .HasDatabaseName(
                "UX_DigitalServices_Predefined_Key")
            .HasFilter("[IsPredefined] = 1")
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.OwnerId,
            x.Key
        })
            .HasDatabaseName(
                "UX_DigitalServices_Custom_OwnerId_Key")
            .HasFilter("[IsPredefined] = 0")
            .IsUnique();
    }
}
