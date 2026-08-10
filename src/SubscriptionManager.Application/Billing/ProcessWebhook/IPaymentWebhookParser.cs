namespace SubscriptionManager.Application.Billing.ProcessWebhook;

public interface IPaymentWebhookParser
{
    PaymentSubscriptionEvent? Parse(
        string payload,
        string signature);
}
