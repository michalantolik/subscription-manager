using Microsoft.EntityFrameworkCore;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptionManagerDbContext).Assembly);
    }
}
