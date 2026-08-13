using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Domain.Billing;
using SubscriptionManager.Infrastructure.Persistence;

namespace SubscriptionManager.Infrastructure.Billing.Persistence;

/// <summary>
/// Applies payment webhook events to billing subscriptions.
/// </summary>
internal sealed class BillingWebhookRepository(
    SubscriptionManagerDbContext dbContext)
    : IBillingWebhookRepository
{
    public async Task<PaymentWebhookProcessingResult> ApplyAsync(
        PaymentSubscriptionEvent paymentEvent,
        DateTimeOffset processedAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            paymentEvent);

        if (processedAt == default)
        {
            throw new ArgumentException(
                "Processing time is required.",
                nameof(processedAt));
        }

        if (await HasBeenProcessedAsync(
                paymentEvent.ProviderEventId,
                cancellationToken))
        {
            return PaymentWebhookProcessingResult.Duplicate;
        }

        IDbContextTransaction? transaction =
            null;

        if (dbContext.Database.IsRelational())
        {
            transaction =
                await dbContext.Database
                    .BeginTransactionAsync(
                        cancellationToken);
        }

        try
        {
            var subscription =
                await FindSubscriptionAsync(
                    paymentEvent,
                    cancellationToken);

            var eventApplied =
                subscription is null
                    ? await CreateSubscriptionAsync(
                        paymentEvent,
                        cancellationToken)
                    : ApplyToExistingSubscription(
                        subscription,
                        paymentEvent);

            dbContext.ProcessedBillingEvents.Add(
                new ProcessedBillingEvent(
                    paymentEvent.ProviderEventId,
                    paymentEvent.ProviderEventCreatedAt,
                    processedAt));

            await dbContext.SaveChangesAsync(
                cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(
                    cancellationToken);
            }

            return eventApplied
                ? PaymentWebhookProcessingResult.Applied
                : PaymentWebhookProcessingResult.Stale;
        }
        catch (DbUpdateException)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(
                    cancellationToken);
            }

            dbContext.ChangeTracker.Clear();

            if (await HasBeenProcessedAsync(
                    paymentEvent.ProviderEventId,
                    cancellationToken))
            {
                return PaymentWebhookProcessingResult.Duplicate;
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    private async Task<BillingSubscription?> FindSubscriptionAsync(
        PaymentSubscriptionEvent paymentEvent,
        CancellationToken cancellationToken)
    {
        var subscription =
            await dbContext.BillingSubscriptions
                .SingleOrDefaultAsync(
                    currentSubscription =>
                        currentSubscription
                            .ProviderSubscriptionId ==
                        paymentEvent.ProviderSubscriptionId,
                    cancellationToken);

        if (subscription is not null ||
            paymentEvent.UserId is null)
        {
            return subscription;
        }

        return await dbContext.BillingSubscriptions
            .SingleOrDefaultAsync(
                currentSubscription =>
                    currentSubscription.UserId ==
                    paymentEvent.UserId.Value,
                cancellationToken);
    }

    private async Task<bool> CreateSubscriptionAsync(
        PaymentSubscriptionEvent paymentEvent,
        CancellationToken cancellationToken)
    {
        if (paymentEvent.UserId is null)
        {
            throw new InvalidOperationException(
                "A new billing subscription cannot be linked to a user.");
        }

        var subscription =
            new BillingSubscription(
                Guid.NewGuid(),
                paymentEvent.UserId.Value,
                paymentEvent.Plan,
                paymentEvent.BillingInterval,
                paymentEvent.CurrentPeriodStart,
                paymentEvent.CurrentPeriodEnd);

        subscription.LinkToPaymentProvider(
            paymentEvent.ProviderCustomerId,
            paymentEvent.ProviderSubscriptionId,
            paymentEvent.ProviderPriceId);

        var applied =
            subscription.ApplyProviderEvent(
                paymentEvent.ProviderEventCreatedAt,
                paymentEvent.Plan,
                paymentEvent.BillingInterval,
                paymentEvent.Status,
                paymentEvent.ProviderPriceId,
                paymentEvent.CurrentPeriodStart,
                paymentEvent.CurrentPeriodEnd,
                paymentEvent.CancelAtPeriodEnd);

        await dbContext.BillingSubscriptions.AddAsync(
            subscription,
            cancellationToken);

        return applied;
    }

    private static bool ApplyToExistingSubscription(
        BillingSubscription subscription,
        PaymentSubscriptionEvent paymentEvent)
    {
        if (paymentEvent.UserId is { } userId &&
            subscription.UserId != userId)
        {
            throw new InvalidOperationException(
                "The billing subscription belongs to a different user.");
        }

        if (subscription.ProviderSubscriptionId is not null &&
            subscription.ProviderSubscriptionId !=
            paymentEvent.ProviderSubscriptionId)
        {
            throw new InvalidOperationException(
                "The user is already linked to a different billing subscription.");
        }

        if (subscription.ProviderCustomerId is not null &&
            subscription.ProviderCustomerId !=
            paymentEvent.ProviderCustomerId)
        {
            throw new InvalidOperationException(
                "The billing subscription is linked to a different payment customer.");
        }

        if (subscription.ProviderSubscriptionId is null ||
            subscription.ProviderCustomerId is null)
        {
            subscription.LinkToPaymentProvider(
                paymentEvent.ProviderCustomerId,
                paymentEvent.ProviderSubscriptionId,
                paymentEvent.ProviderPriceId);
        }

        return subscription.ApplyProviderEvent(
            paymentEvent.ProviderEventCreatedAt,
            paymentEvent.Plan,
            paymentEvent.BillingInterval,
            paymentEvent.Status,
            paymentEvent.ProviderPriceId,
            paymentEvent.CurrentPeriodStart,
            paymentEvent.CurrentPeriodEnd,
            paymentEvent.CancelAtPeriodEnd);
    }

    private async Task<bool> HasBeenProcessedAsync(
        string providerEventId,
        CancellationToken cancellationToken)
    {
        return await dbContext.ProcessedBillingEvents
            .AsNoTracking()
            .AnyAsync(
                billingEvent =>
                    billingEvent.ProviderEventId ==
                    providerEventId,
                cancellationToken);
    }
}
