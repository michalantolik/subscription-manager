using Microsoft.AspNetCore.Identity;
using SubscriptionManager.Domain.Subscriptions;

namespace SubscriptionManager.Infrastructure.Identity;

public sealed class ApplicationUser
    : IdentityUser<Guid>
{
    public Currency BaseCurrency { get; set; } =
        Currency.PLN;
}
