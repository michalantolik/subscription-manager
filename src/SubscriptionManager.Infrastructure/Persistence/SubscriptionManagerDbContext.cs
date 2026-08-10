using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Domain.DigitalServices;
using SubscriptionManager.Domain.ExchangeRates;
using SubscriptionManager.Domain.SavingsPlans;
using SubscriptionManager.Domain.Subscriptions;
using SubscriptionManager.Infrastructure.Billing;
using SubscriptionManager.Infrastructure.Identity;

namespace SubscriptionManager.Infrastructure.Persistence;

public sealed class SubscriptionManagerDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public SubscriptionManagerDbContext(
        DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Subscription> Subscriptions =>
        Set<Subscription>();

    public DbSet<DigitalService> DigitalServices =>
        Set<DigitalService>();

    public DbSet<ExchangeRate> ExchangeRates =>
        Set<ExchangeRate>();

    public DbSet<SavingsPlanUsage> SavingsPlanUsages =>
        Set<SavingsPlanUsage>();

    public DbSet<BillingSubscription> BillingSubscriptions =>
        Set<BillingSubscription>();

    internal DbSet<ProcessedBillingEvent> ProcessedBillingEvents =>
        Set<ProcessedBillingEvent>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(
            modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SubscriptionManagerDbContext).Assembly);
    }
}
