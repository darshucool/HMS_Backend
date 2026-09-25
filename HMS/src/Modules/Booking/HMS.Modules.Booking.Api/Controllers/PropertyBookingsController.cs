using HMS.Modules.Booking.Api.Contracts;
using HMS.Modules.Booking.Application.Commands.CreateBooking;
using HMS.Modules.Booking.Application.Queries.GetBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Booking.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/bookings")]
[Authorize]
public sealed class PropertyBookingsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingsQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateBookingCommand(
            propertyUid,
            request.LeadGuestUid,
            request.BookingSource,
            request.CheckInDate,
            request.CheckOutDate,
            request.Adults,
            request.Children,
            request.Infants,
            request.Currency,
            request.DiscountAmount,
            request.TaxAmount,
            request.ServiceCharge,
            request.ArrivalTime,
            request.DepartureTime,
            request.SpecialRequests,
            request.InternalNotes,
            request.ExternalReference,
            request.Units?.Select(unit => new CreateBookingUnit(
                unit.AccommodationTypeUid,
                unit.PricingBasis,
                unit.UnitRate,
                unit.UnitUid,
                unit.RatePlanUid,
                unit.MealPlanUid,
                unit.Adults,
                unit.Children,
                unit.UnitQuantity,
                unit.GuestCount,
                unit.DiscountAmount,
                unit.TaxAmount,
                unit.Notes)).ToList() ?? [],
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
