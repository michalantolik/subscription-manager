namespace SubscriptionManager.Application.Authentication;

/// <summary>
/// Generates access tokens used to authenticate application users.
/// </summary>
public interface IAccessTokenGenerator
{
    string GenerateToken(Guid userId);
}
