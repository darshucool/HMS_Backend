using HMS.Modules.Guests.Api.Contracts;
using HMS.Modules.Guests.Application.Commands.CreateGuest;
using HMS.Modules.Guests.Application.Queries.GetGuests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Guests.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
[Authorize]
public sealed class PropertiesGuestsController(ISender sender) : ControllerBase
{
    [HttpGet("{propertyUid:guid}/guests")]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGuestsQuery(
            propertyUid,
            search,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{propertyUid:guid}/guests")]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] CreateGuestRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateGuestCommand(
            propertyUid,
            request.GuestType,
            request.Title,
            request.FirstName,
            request.LastName,
            request.DisplayName,
            request.Phone,
            request.AlternatePhone,
            request.Email,
            request.NationalityCode,
            request.DateOfBirth,
            request.PreferredLanguage,
            request.Address,
            request.City,
            request.CountryCode,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
