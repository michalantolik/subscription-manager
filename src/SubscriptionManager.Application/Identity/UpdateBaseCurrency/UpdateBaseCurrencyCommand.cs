using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Application.Identity.UpdateBaseCurrency;

public sealed record UpdateBaseCurrencyCommand(
    Guid UserId,
    Currency BaseCurrency);
