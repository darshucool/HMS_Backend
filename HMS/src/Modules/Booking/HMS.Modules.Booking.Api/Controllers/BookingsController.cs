using HMS.Modules.Booking.Api.Contracts;
using HMS.Modules.Booking.Application.Commands.AssignBookingUnit;
using HMS.Modules.Booking.Application.Commands.ChangeBookingStatus;
using HMS.Modules.Booking.Application.Commands.UpdateBooking;
using HMS.Modules.Booking.Application.Queries.GetBooking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Booking.Api.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Authorize]
public sealed class BookingsController(ISender sender) : ControllerBase
{
    [HttpGet("{bookingUid:guid}")]
    public async Task<IActionResult> Get(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingQuery(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{bookingUid:guid}")]
    public async Task<IActionResult> Update(
        Guid bookingUid,
        [FromBody] UpdateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateBookingCommand(
            bookingUid,
            request.LeadGuestUid,
            request.BookingSource,
            request.Status,
            request.CheckInDate,
            request.CheckOutDate,
            request.Adults,
            request.Children,
            request.Infants,
            request.Currency,
            request.DiscountAmount,
            request.TaxAmount,
            request.ServiceCharge,
            request.QuotedTotal,
            request.ArrivalTime,
            request.DepartureTime,
            request.SpecialRequests,
            request.InternalNotes,
            request.ExternalReference,
            request.CancellationReason,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{bookingUid:guid}/confirm")]
    public async Task<IActionResult> Confirm(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmBookingCommand(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{bookingUid:guid}/check-in")]
    public async Task<IActionResult> CheckIn(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CheckInBookingCommand(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{bookingUid:guid}/check-out")]
    public async Task<IActionResult> CheckOut(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CheckOutBookingCommand(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{bookingUid:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid bookingUid,
        [FromBody] CancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelBookingCommand(
            bookingUid,
            request.Reason,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{bookingUid:guid}/assign-unit")]
    public async Task<IActionResult> AssignUnit(
        Guid bookingUid,
        [FromBody] AssignBookingUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AssignBookingUnitCommand(
            bookingUid,
            request.BookingUnitUid,
            request.UnitUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}
