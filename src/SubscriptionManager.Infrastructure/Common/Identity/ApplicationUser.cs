using Microsoft.AspNetCore.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Infrastructure.Common.Identity;

/// <summary>
/// Represents an application user with account preferences.
/// </summary>
public sealed class ApplicationUser
    : IdentityUser<Guid>
{
    public Language Language { get; set; } =
        Language.Polish;

    public Currency BaseCurrency { get; set; } =
        Currency.PLN;
}
