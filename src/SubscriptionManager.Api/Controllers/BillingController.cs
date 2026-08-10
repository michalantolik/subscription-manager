using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.Billing.CreateCheckoutSession;
using SubscriptionManager.Application.Billing.GetBillingOverview;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize]
public sealed class BillingController(
    GetBillingOverviewHandler getBillingOverviewHandler,
    CreateCheckoutSessionHandler createCheckoutSessionHandler)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BillingOverviewDto>> GetBillingOverviewAsync(
        CancellationToken cancellationToken)
    {
        var billingOverview =
            await getBillingOverviewHandler.HandleAsync(
                cancellationToken);

        return Ok(
            billingOverview);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CreateCheckoutSessionResponse>> CreateCheckoutSessionAsync(
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
}

public sealed record CreateCheckoutSessionRequest(
    SubscriptionManager.Domain.Billing.SubscriptionPlan Plan,
    SubscriptionManager.Domain.Billing.BillingInterval BillingInterval,
    string SuccessUrl,
    string CancelUrl);

public sealed record CreateCheckoutSessionResponse(
    string CheckoutUrl);
