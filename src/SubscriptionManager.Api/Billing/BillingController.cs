using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Billing.CancelSubscription;
using SubscriptionManager.Application.Billing.ChangeSubscription;
using SubscriptionManager.Application.Billing.CreateCheckoutSession;
using SubscriptionManager.Application.Billing.GetBillingOverview;
using SubscriptionManager.Application.Billing.GetPaymentPlans;
using SubscriptionManager.Application.Billing.PaymentProvider;
using SubscriptionManager.Application.Billing.PreviewSubscriptionChange;
using SubscriptionManager.Application.Billing.ProcessWebhook;
using SubscriptionManager.Application.Billing.ResumeSubscription;
using SubscriptionManager.Domain.Billing;
using System.Text;

namespace SubscriptionManager.Api.Billing;

/// <summary>
/// Exposes billing use cases through HTTP endpoints.
/// </summary>
[ApiController]
[Route("api/billing")]
[Authorize]
public sealed class BillingController(
    GetBillingOverviewHandler getBillingOverviewHandler,
    GetPaymentPlansHandler getPaymentPlansHandler,
    CreateCheckoutSessionHandler createCheckoutSessionHandler,
    PreviewSubscriptionChangeHandler previewSubscriptionChangeHandler,
    ChangeSubscriptionHandler changeSubscriptionHandler,
    CancelSubscriptionHandler cancelSubscriptionHandler,
    ResumeSubscriptionHandler resumeSubscriptionHandler,
    ProcessPaymentWebhookHandler processPaymentWebhookHandler)
    : ControllerBase
{
    private const string StripeSignatureHeader =
        "Stripe-Signature";

    [HttpGet]
    public async Task<ActionResult<BillingOverviewDto>>
        GetBillingOverviewAsync(
            CancellationToken cancellationToken)
    {
        var billingOverview =
            await getBillingOverviewHandler.HandleAsync(
                cancellationToken);

        return Ok(
            billingOverview);
    }

    [AllowAnonymous]
    [HttpGet("plans")]
    public async Task<ActionResult<IReadOnlyList<PaymentPlanPrice>>>
        GetPaymentPlansAsync(
            CancellationToken cancellationToken)
    {
        var plans =
            await getPaymentPlansHandler.HandleAsync(
                cancellationToken);

        return Ok(
            plans);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CreateCheckoutSessionResponse>>
        CreateCheckoutSessionAsync(
            CreateCheckoutSessionRequest request,
            CancellationToken cancellationToken)
    {
        var checkoutUrl =
            await createCheckoutSessionHandler.HandleAsync(
                new CreateCheckoutSessionCommand(
                    request.Plan,
                    request.BillingInterval,
                    request.SuccessUrl,
                    request.CancelUrl),
                cancellationToken);

        if (checkoutUrl is null)
        {
            return Unauthorized();
        }

        return Ok(
            new CreateCheckoutSessionResponse(
                checkoutUrl.ToString()));
    }

    [HttpPost("subscription/change-preview")]
    public async Task<ActionResult<SubscriptionChangePreviewDto>>
        PreviewSubscriptionChangeAsync(
            PreviewSubscriptionChangeRequest request,
            CancellationToken cancellationToken)
    {
        var preview =
            await previewSubscriptionChangeHandler.HandleAsync(
                new PreviewSubscriptionChangeCommand(
                    request.Plan,
                    request.BillingInterval),
                cancellationToken);

        return Ok(
            preview);
    }

    [HttpPost("subscription/change")]
    public async Task<IActionResult> ChangeSubscriptionAsync(
        ChangeSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        await changeSubscriptionHandler.HandleAsync(
            new ChangeSubscriptionCommand(
                request.Plan,
                request.BillingInterval),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("subscription/cancel")]
    public async Task<IActionResult> CancelSubscriptionAsync(
        CancellationToken cancellationToken)
    {
        await cancelSubscriptionHandler.HandleAsync(
            new CancelSubscriptionCommand(),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("subscription/resume")]
    public async Task<IActionResult> ResumeSubscriptionAsync(
        CancellationToken cancellationToken)
    {
        await resumeSubscriptionHandler.HandleAsync(
            new ResumeSubscriptionCommand(),
            cancellationToken);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> ProcessWebhookAsync(
        CancellationToken cancellationToken)
    {
        var signature =
            Request.Headers[
                StripeSignatureHeader]
                .ToString();

        using var reader =
            new StreamReader(
                Request.Body,
                Encoding.UTF8);

        var payload =
            await reader.ReadToEndAsync(
                cancellationToken);

        await processPaymentWebhookHandler.HandleAsync(
            new ProcessPaymentWebhookCommand(
                payload,
                signature),
            cancellationToken);

        return NoContent();
    }
}

/// <summary>
/// Checkout session data accepted by the API.
/// </summary>
public sealed record CreateCheckoutSessionRequest(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval,
    string SuccessUrl,
    string CancelUrl);

/// <summary>
/// Checkout session data returned by the API.
/// </summary>
public sealed record CreateCheckoutSessionResponse(
    string CheckoutUrl);

/// <summary>
/// Subscription change preview data accepted by the API.
/// </summary>
public sealed record PreviewSubscriptionChangeRequest(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval);

/// <summary>
/// Subscription change data accepted by the API.
/// </summary>
public sealed record ChangeSubscriptionRequest(
    SubscriptionPlan Plan,
    BillingInterval BillingInterval);
