using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using MediatR;

namespace HMS.Modules.Booking.Application.Queries.GetBookings;

public sealed record GetBookingsQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<IReadOnlyList<BookingDto>>>;

public sealed class GetBookingsQueryHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingsQuery, BookingResult<IReadOnlyList<BookingDto>>>
{
    public async Task<BookingResult<IReadOnlyList<BookingDto>>> Handle(
        GetBookingsQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return BookingResult<IReadOnlyList<BookingDto>>.Forbidden(
                "You cannot access bookings for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return BookingResult<IReadOnlyList<BookingDto>>.NotFound("Property was not found.");

        var items = await bookingRepository.GetByPropertyUidAsync(request.PropertyUid, cancellationToken);
        return BookingResult<IReadOnlyList<BookingDto>>.Success(items);
    }
}
