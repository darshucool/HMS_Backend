using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using MediatR;

namespace HMS.Modules.Booking.Application.Queries.GetBookingCalendar;

public sealed record GetBookingCalendarQuery(
    Guid PropertyUid,
    DateOnly From,
    DateOnly To,
    Guid? AccommodationTypeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingCalendarDto>>;

public sealed class GetBookingCalendarQueryHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingCalendarQuery, BookingResult<BookingCalendarDto>>
{
    public async Task<BookingResult<BookingCalendarDto>> Handle(
        GetBookingCalendarQuery request,
        CancellationToken cancellationToken)
    {
        if (request.To <= request.From)
            return BookingResult<BookingCalendarDto>.Validation("The end date must be after the start date.");

        if (request.To.DayNumber - request.From.DayNumber > 366)
            return BookingResult<BookingCalendarDto>.Validation("The calendar range cannot exceed 366 days.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return BookingResult<BookingCalendarDto>.Forbidden(
                "You cannot view the booking calendar for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return BookingResult<BookingCalendarDto>.NotFound("Property was not found.");

        long? accommodationTypeId = null;
        if (request.AccommodationTypeUid is Guid accommodationTypeUid)
        {
            var accommodationType = await bookingRepository.GetAccommodationTypeAsync(
                accommodationTypeUid,
                property.Id,
                cancellationToken);

            if (accommodationType is null)
                return BookingResult<BookingCalendarDto>.NotFound("Accommodation type was not found.");

            accommodationTypeId = accommodationType.Id;
        }

        var calendar = await bookingRepository.GetBookingCalendarAsync(
            property.Id,
            request.PropertyUid,
            request.From,
            request.To,
            accommodationTypeId,
            cancellationToken);

        return BookingResult<BookingCalendarDto>.Success(calendar);
    }
}
