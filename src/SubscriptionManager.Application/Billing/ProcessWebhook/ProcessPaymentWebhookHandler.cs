namespace SubscriptionManager.Application.Billing.ProcessWebhook;

public sealed class ProcessPaymentWebhookHandler(
    IPaymentWebhookParser paymentWebhookParser,
    IBillingWebhookRepository billingWebhookRepository,
    TimeProvider timeProvider)
{
    public async Task<PaymentWebhookProcessingResult> HandleAsync(
        ProcessPaymentWebhookCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(
                command.Payload))
        {
            throw new InvalidPaymentWebhookException(
                "The payment webhook payload is missing.");
        }

        if (string.IsNullOrWhiteSpace(
                command.Signature))
        {
            throw new InvalidPaymentWebhookException(
                "The payment webhook signature is missing.");
        }

        var paymentEvent =
            paymentWebhookParser.Parse(
                command.Payload,
                command.Signature);

        if (paymentEvent is null)
        {
            return PaymentWebhookProcessingResult.Ignored;
        }

        return await billingWebhookRepository.ApplyAsync(
            paymentEvent,
            timeProvider.GetUtcNow(),
            cancellationToken);
    }
}
