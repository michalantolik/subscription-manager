namespace SubscriptionManager.Application.Common.Identity;

public interface ICurrentUser
{
    Guid UserId { get; }
}
