using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.UpdateAccommodationUnit;
using HMS.Modules.Hotels.Application.Queries.GetAccommodationUnit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/units")]
[Authorize]
public sealed class UnitsController(ISender sender) : ControllerBase
{
    [HttpGet("{uid:guid}")]
    public async Task<IActionResult> Get(
        Guid uid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAccommodationUnitQuery(
            uid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{uid:guid}")]
    public async Task<IActionResult> Update(
        Guid uid,
        [FromBody] UpdateAccommodationUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateAccommodationUnitCommand(
            uid,
            request.AccommodationTypeUid,
            request.UnitCode,
            request.UnitName,
            request.FloorOrArea,
            request.Status,
            request.HousekeepingStatus,
            request.Notes,
            request.IsActive,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}
