using Microsoft.AspNetCore.Mvc;
using SubscriptionManager.Application.DigitalServices;
using SubscriptionManager.Application.DigitalServices.CreateDigitalService;
using SubscriptionManager.Application.DigitalServices.DeactivateDigitalService;
using SubscriptionManager.Application.DigitalServices.DeleteDigitalService;
using SubscriptionManager.Application.DigitalServices.GetDigitalServiceById;
using SubscriptionManager.Application.DigitalServices.GetDigitalServices;
using SubscriptionManager.Application.DigitalServices.UpdateDigitalService;
using SubscriptionManager.Domain.DigitalServices;

namespace SubscriptionManager.Api.Controllers;

[ApiController]
[Route("api/digital-services")]
public sealed class DigitalServicesController : ControllerBase
{
    private const string GetDigitalServiceByIdRouteName =
        "GetDigitalServiceById";

    private readonly CreateDigitalServiceHandler _createDigitalServiceHandler;
    private readonly GetDigitalServicesHandler _getDigitalServicesHandler;
    private readonly GetDigitalServiceByIdHandler _getDigitalServiceByIdHandler;
    private readonly UpdateDigitalServiceHandler _updateDigitalServiceHandler;
    private readonly DeactivateDigitalServiceHandler _deactivateDigitalServiceHandler;
    private readonly DeleteDigitalServiceHandler _deleteDigitalServiceHandler;

    public DigitalServicesController(
        CreateDigitalServiceHandler createDigitalServiceHandler,
        GetDigitalServicesHandler getDigitalServicesHandler,
        GetDigitalServiceByIdHandler getDigitalServiceByIdHandler,
        UpdateDigitalServiceHandler updateDigitalServiceHandler,
        DeactivateDigitalServiceHandler deactivateDigitalServiceHandler,
        DeleteDigitalServiceHandler deleteDigitalServiceHandler)
    {
        _createDigitalServiceHandler = createDigitalServiceHandler;
        _getDigitalServicesHandler = getDigitalServicesHandler;
        _getDigitalServiceByIdHandler = getDigitalServiceByIdHandler;
        _updateDigitalServiceHandler = updateDigitalServiceHandler;
        _deactivateDigitalServiceHandler = deactivateDigitalServiceHandler;
        _deleteDigitalServiceHandler = deleteDigitalServiceHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DigitalServiceDto>>> GetAsync(
        CancellationToken cancellationToken)
    {
        var digitalServices = await _getDigitalServicesHandler.HandleAsync(
            cancellationToken);

        return Ok(digitalServices);
    }

    [HttpGet("{id:guid}", Name = GetDigitalServiceByIdRouteName)]
    public async Task<ActionResult<DigitalServiceDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var digitalService = await _getDigitalServiceByIdHandler.HandleAsync(
            id,
            cancellationToken);

        if (digitalService is null)
        {
            return NotFound();
        }

        return Ok(digitalService);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateAsync(
        CreateDigitalServiceCommand command,
        CancellationToken cancellationToken)
    {
        var digitalServiceId = await _createDigitalServiceHandler.HandleAsync(
            command,
            cancellationToken);

        return CreatedAtRoute(
            GetDigitalServiceByIdRouteName,
            new { id = digitalServiceId },
            digitalServiceId);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        UpdateDigitalServiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateDigitalServiceCommand(
            id,
            request.Key,
            request.Name,
            request.Category,
            request.CustomCategoryName,
            request.IconKey,
            request.ManagementUrl);

        var updated = await _updateDigitalServiceHandler.HandleAsync(
            command,
            cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateDigitalServiceCommand(id);

        var deactivated = await _deactivateDigitalServiceHandler.HandleAsync(
            command,
            cancellationToken);

        if (!deactivated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDigitalServiceCommand(id);

        var deleted = await _deleteDigitalServiceHandler.HandleAsync(
            command,
            cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

public sealed record UpdateDigitalServiceRequest(
    string Key,
    string Name,
    DigitalServiceCategory Category,
    string? CustomCategoryName,
    string? IconKey,
    string? ManagementUrl);
