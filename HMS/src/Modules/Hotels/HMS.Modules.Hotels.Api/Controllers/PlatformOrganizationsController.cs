using HMS.Modules.Hotels.Api.Authorization;
using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.CreateOrganization;
using HMS.Modules.Hotels.Application.Queries.GetOrganizations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/platform/organizations")]
[Authorize(Policy = HotelsPolicies.PlatformAdmin)]
public sealed class PlatformOrganizationsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOrganizationCommand(
            request.Code,
            request.Name,
            request.LegalName,
            request.DefaultCurrency,
            request.Timezone,
            User.GetRequiredSubject()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetOrganizationsQuery(page, pageSize, search),
            cancellationToken);

        return this.ToActionResult(result);
    }
}
