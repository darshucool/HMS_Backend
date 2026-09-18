using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.DeleteAccommodationType;
using HMS.Modules.Hotels.Application.Commands.UpdateAccommodationType;
using HMS.Modules.Hotels.Application.Queries.GetAccommodationType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/accommodation-types")]
[Authorize]
public sealed class AccommodationTypesController(ISender sender) : ControllerBase
{
    [HttpGet("{uid:guid}")]
    public async Task<IActionResult> Get(
        Guid uid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAccommodationTypeQuery(
            uid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{uid:guid}")]
    public async Task<IActionResult> Update(
        Guid uid,
        [FromBody] UpdateAccommodationTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateAccommodationTypeCommand(
            uid,
            request.Code,
            request.Name,
            request.UnitKind,
            request.Description,
            request.MaxAdults,
            request.MaxChildren,
            request.MaxOccupancy,
            request.DefaultQuantity,
            request.BaseRate,
            request.SortOrder,
            request.IsActive,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{uid:guid}")]
    public async Task<IActionResult> Delete(
        Guid uid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteAccommodationTypeCommand(
            uid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }
}
