using HMS.Modules.Hotels.Api.Authorization;
using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.CreateProperty;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/organizations/{organizationUid:guid}/properties")]
[Authorize(Policy = HotelsPolicies.PlatformAdmin)]
public sealed class OrganizationPropertiesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        Guid organizationUid,
        [FromBody] CreatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreatePropertyCommand(
            organizationUid,
            request.Code,
            request.Name,
            request.Slug,
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
            User.GetRequiredSubject()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
