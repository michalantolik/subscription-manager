using Microsoft.AspNetCore.Identity;
using SubscriptionManager.Application.Common.Identity;
using SubscriptionManager.Application.Common.Localization;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Infrastructure.Identity;

public sealed class ApplicationUser
    : IdentityUser<Guid>
{
    public Language Language { get; set; } =
        Language.Polish;

    public Currency BaseCurrency { get; set; } =
        Currency.PLN;

    public SubscriptionPlan SubscriptionPlan { get; set; } =
        SubscriptionPlan.Free;
}
