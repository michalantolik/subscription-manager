namespace SubscriptionManager.Application.Common.Identity;

public interface ICurrentUser
{
    /// <summary>
    /// Provides the current application user's identity.
    /// </summary>
    Guid UserId { get; }
}
