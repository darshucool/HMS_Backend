using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using HMS.Modules.Booking.Domain.Enums;
using MediatR;
using HotelBooking = HMS.Modules.Booking.Domain.Entities.Booking;

namespace HMS.Modules.Booking.Application.Commands.CreateBooking;

public sealed record CreateBookingUnit(
    Guid AccommodationTypeUid,
    PricingBasis PricingBasis,
    decimal UnitRate,
    Guid? UnitUid = null,
    Guid? RatePlanUid = null,
    Guid? MealPlanUid = null,
    int Adults = 1,
    int Children = 0,
    int UnitQuantity = 1,
    int GuestCount = 1,
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    string? Notes = null);

public sealed record CreateBookingCommand(
    Guid PropertyUid,
    Guid LeadGuestUid,
    BookingSource BookingSource,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    int Infants,
    string Currency,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    TimeOnly? ArrivalTime,
    TimeOnly? DepartureTime,
    string? SpecialRequests,
    string? InternalNotes,
    string? ExternalReference,
    IReadOnlyList<CreateBookingUnit> Units,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDto>>;

public sealed class CreateBookingCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<CreateBookingCommand, BookingResult<BookingDto>>
{
    public async Task<BookingResult<BookingDto>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return BookingResult<BookingDto>.Forbidden("You cannot create bookings for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return BookingResult<BookingDto>.NotFound("Property was not found.");

        var guest = await bookingRepository.GetGuestAsync(request.LeadGuestUid, cancellationToken);
        if (guest is null)
            return BookingResult<BookingDto>.NotFound("Lead guest was not found.");

        if (guest.OrganizationId != property.OrganizationId)
            return BookingResult<BookingDto>.Validation("Lead guest does not belong to this property's organization.");

        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
        var lines = new List<BookingUnitLine>();
        decimal unitsTotal = 0;

        foreach (var unit in request.Units)
        {
            if (unit.UnitQuantity <= 0 || unit.GuestCount <= 0 || unit.Adults < 0 || unit.Children < 0)
                return BookingResult<BookingDto>.Validation("Each booking unit needs a positive quantity and guest count.");

            if (unit.UnitRate < 0 || unit.DiscountAmount < 0 || unit.TaxAmount < 0)
                return BookingResult<BookingDto>.Validation("Booking unit amounts cannot be negative.");

            var accommodationType = await bookingRepository.GetAccommodationTypeAsync(
                unit.AccommodationTypeUid,
                property.Id,
                cancellationToken);

            if (accommodationType is null)
                return BookingResult<BookingDto>.NotFound("Accommodation type was not found for this property.");

            long? unitId = null;
            if (unit.UnitUid is Guid unitUid)
            {
                unitId = await bookingRepository.GetUnitIdAsync(
                    unitUid,
                    property.Id,
                    accommodationType.Id,
                    cancellationToken);

                if (unitId is null)
                    return BookingResult<BookingDto>.NotFound("Accommodation unit was not found for this type.");
            }

            long? ratePlanId = null;
            if (unit.RatePlanUid is Guid ratePlanUid)
            {
                ratePlanId = await bookingRepository.GetRatePlanIdAsync(ratePlanUid, property.Id, cancellationToken);
                if (ratePlanId is null)
                    return BookingResult<BookingDto>.NotFound("Rate plan was not found for this property.");
            }

            long? mealPlanId = null;
            if (unit.MealPlanUid is Guid mealPlanUid)
            {
                mealPlanId = await bookingRepository.GetMealPlanIdAsync(mealPlanUid, property.Id, cancellationToken);
                if (mealPlanId is null)
                    return BookingResult<BookingDto>.NotFound("Meal plan was not found for this property.");
            }

            var stayCharge = unit.PricingBasis == PricingBasis.FlatPerStay
                ? unit.UnitRate * unit.UnitQuantity
                : unit.UnitRate * nights * unit.UnitQuantity;
            var total = stayCharge - unit.DiscountAmount + unit.TaxAmount;
            if (total < 0)
                return BookingResult<BookingDto>.Validation("Booking unit total cannot be negative.");

            unitsTotal += total;
            lines.Add(new BookingUnitLine(
                accommodationType.Id,
                unitId,
                ratePlanId,
                mealPlanId,
                request.CheckInDate,
                request.CheckOutDate,
                unit.Adults,
                unit.Children,
                unit.UnitQuantity,
                unit.GuestCount,
                unit.PricingBasis.ToDatabaseValue(),
                unit.UnitRate,
                unit.DiscountAmount,
                unit.TaxAmount,
                total,
                unit.Notes));
        }

        var quotedTotal = lines.Count == 0
            ? (decimal?)null
            : unitsTotal - request.DiscountAmount + request.TaxAmount + request.ServiceCharge;

        var bookingNumber = await bookingRepository.NextBookingNumberAsync(
            property.Id,
            property.BookingNumberPrefix,
            cancellationToken);

        HotelBooking booking;
        try
        {
            booking = HotelBooking.Create(
                property.OrganizationId,
                property.Id,
                request.PropertyUid,
                bookingNumber,
                guest.Id,
                request.LeadGuestUid,
                request.BookingSource,
                request.ExternalReference,
                request.CheckInDate,
                request.CheckOutDate,
                request.Adults,
                request.Children,
                request.Infants,
                request.Currency,
                request.DiscountAmount,
                request.TaxAmount,
                request.ServiceCharge,
                quotedTotal,
                request.ArrivalTime,
                request.DepartureTime,
                request.SpecialRequests,
                request.InternalNotes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return BookingResult<BookingDto>.Validation(exception.Message);
        }

        try
        {
            await bookingRepository.InsertAsync(booking, lines, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BookingResult<BookingDto>.Conflict(exception.Message);
        }

        return BookingResult<BookingDto>.Success(new BookingDto(
            booking.Uid,
            booking.PropertyUid,
            booking.BookingNumber,
            booking.LeadGuestUid,
            null,
            booking.BookingSource.ToDatabaseValue(),
            "PENDING",
            booking.CheckInDate,
            booking.CheckOutDate,
            nights,
            booking.Adults,
            booking.Children,
            booking.Infants,
            booking.Currency,
            booking.QuotedTotal,
            booking.SpecialRequests,
            booking.CreationDate));
    }
}
