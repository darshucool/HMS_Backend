using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.UpdateProperty;
using HMS.Modules.Hotels.Application.Commands.UpdatePropertySettings;
using HMS.Modules.Hotels.Application.Queries.GetProperty;
using HMS.Modules.Hotels.Application.Queries.GetPropertySettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
[Authorize]
public sealed class PropertiesController(ISender sender) : ControllerBase
{
    [HttpGet("{propertyUid:guid}")]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPropertyQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{propertyUid:guid}")]
    public async Task<IActionResult> Update(
        Guid propertyUid,
        [FromBody] UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePropertyCommand(
            propertyUid,
            request.Name,
            request.PropertyType,
            request.Description,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.District,
            request.Province,
            request.PostalCode,
            request.CountryCode,
            request.Latitude,
            request.Longitude,
            request.Phone,
            request.Email,
            request.Timezone,
            request.DefaultCurrency,
            request.Status,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{propertyUid:guid}/settings")]
    public async Task<IActionResult> GetSettings(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPropertySettingsQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{propertyUid:guid}/settings")]
    public async Task<IActionResult> UpdateSettings(
        Guid propertyUid,
        [FromBody] UpdatePropertySettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePropertySettingsCommand(
            propertyUid,
            request.CheckInTime,
            request.CheckOutTime,
            request.BookingNumberPrefix,
            request.InvoiceNumberPrefix,
            request.TaxRate,
            request.ServiceChargeRate,
            request.AllowOverbooking,
            request.ExtraSettings,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}

