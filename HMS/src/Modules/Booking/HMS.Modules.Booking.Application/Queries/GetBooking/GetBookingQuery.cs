using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using MediatR;

namespace HMS.Modules.Booking.Application.Queries.GetBooking;

public sealed record GetBookingQuery(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDetailDto>>;

public sealed class GetBookingQueryHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingQuery, BookingResult<BookingDetailDto>>
{
    public async Task<BookingResult<BookingDetailDto>> Handle(
        GetBookingQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return BookingResult<BookingDetailDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                booking.PropertyUid,
                false,
                cancellationToken))
        {
            return BookingResult<BookingDetailDto>.Forbidden("You cannot access this booking.");
        }

        var detail = await bookingRepository.GetDetailByUidAsync(request.BookingUid, cancellationToken);
        return detail is null
            ? BookingResult<BookingDetailDto>.NotFound("Booking was not found.")
            : BookingResult<BookingDetailDto>.Success(detail);
    }
}
