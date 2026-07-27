namespace SubscriptionManager.Application.Common.Authentication;

public interface ICurrentUser
{
    Guid UserId { get; }
}
