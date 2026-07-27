using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Infrastructure.Persistence;

public sealed class SubscriptionManagerDbContext : DbContext
{
    public SubscriptionManagerDbContext(
        DbContextOptions<SubscriptionManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<DigitalService> DigitalServices => Set<DigitalService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SubscriptionManagerDbContext).Assembly);
    }
}
