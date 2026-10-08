using HMS.Modules.Booking.Application.Queries.GetBookingCalendar;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Booking.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}")]
[Authorize]
public sealed class PropertyBookingCalendarController(ISender sender) : ControllerBase
{
    [HttpGet("booking-calendar")]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        [FromQuery] Guid? accommodationTypeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingCalendarQuery(
            propertyUid,
            from,
            to,
            accommodationTypeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}
