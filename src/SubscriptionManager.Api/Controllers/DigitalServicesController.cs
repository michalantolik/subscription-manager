using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.GetDigitalServices;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/digital-services")]
public sealed class DigitalServicesController : ControllerBase
{
    private readonly GetDigitalServicesHandler _getDigitalServicesHandler;

    public DigitalServicesController(
        GetDigitalServicesHandler getDigitalServicesHandler)
    {
        _getDigitalServicesHandler = getDigitalServicesHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DigitalServiceDto>>> GetAsync(
        CancellationToken cancellationToken)
    {
        var digitalServices = await _getDigitalServicesHandler.HandleAsync(
            cancellationToken);

        return Ok(digitalServices);
    }
}
