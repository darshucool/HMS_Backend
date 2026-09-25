using HMS.Modules.Guests.Api.Contracts;
using HMS.Modules.Guests.Application.Commands.CreateGuestDocument;
using HMS.Modules.Guests.Application.Commands.CreateGuestPreference;
using HMS.Modules.Guests.Application.Commands.UpdateGuest;
using HMS.Modules.Guests.Application.Queries.GetGuest;
using HMS.Modules.Guests.Application.Queries.GetGuestBookingHistory;
using HMS.Modules.Guests.Application.Queries.SearchGuests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Guests.Api.Controllers;

[ApiController]
[Route("api/v1/guests")]
[Authorize]
public sealed class GuestsController(ISender sender) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? phone,
        [FromQuery] string? name,
        [FromQuery] string? email,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchGuestsQuery(
            phone,
            name,
            email,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{guestUid:guid}")]
    public async Task<IActionResult> Get(
        Guid guestUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGuestQuery(
            guestUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{guestUid:guid}")]
    public async Task<IActionResult> Update(
        Guid guestUid,
        [FromBody] UpdateGuestRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateGuestCommand(
            guestUid,
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
            request.IsActive,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{guestUid:guid}/documents")]
    public async Task<IActionResult> CreateDocument(
        Guid guestUid,
        [FromBody] CreateGuestDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateGuestDocumentCommand(
            guestUid,
            request.DocumentType,
            request.DocumentNumber,
            request.IssuingCountry,
            request.IssuedDate,
            request.ExpiryDate,
            request.FileUrl,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("{guestUid:guid}/preferences")]
    public async Task<IActionResult> CreatePreference(
        Guid guestUid,
        [FromBody] CreateGuestPreferenceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateGuestPreferenceCommand(
            guestUid,
            request.PreferenceType,
            request.PreferenceKey,
            request.PreferenceValue,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{guestUid:guid}/booking-history")]
    public async Task<IActionResult> GetBookingHistory(
        Guid guestUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGuestBookingHistoryQuery(
            guestUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}
