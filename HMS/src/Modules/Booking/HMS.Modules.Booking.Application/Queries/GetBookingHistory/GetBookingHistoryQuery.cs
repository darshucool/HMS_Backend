using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using HMS.Modules.Booking.Domain.Enums;
using MediatR;

namespace HMS.Modules.Booking.Application.Queries.GetBookingHistory;

public sealed record GetBookingHistoryQuery(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingHistoryDto>>;

public sealed class GetBookingHistoryQueryHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<GetBookingHistoryQuery, BookingResult<BookingHistoryDto>>
{
    public async Task<BookingResult<BookingHistoryDto>> Handle(
        GetBookingHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return BookingResult<BookingHistoryDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                booking.PropertyUid,
                false,
                cancellationToken))
        {
            return BookingResult<BookingHistoryDto>.Forbidden("You cannot view this booking's history.");
        }

        var changes = await bookingRepository.GetStatusHistoryAsync(booking.Id, cancellationToken);
        var createdStatus = changes.Count > 0 && !string.IsNullOrWhiteSpace(changes[0].OldStatus)
            ? changes[0].OldStatus!
            : booking.Status.ToDatabaseValue();

        var statusChanges = new List<BookingStatusHistoryDto>
        {
            new(
                Guid.Empty,
                null,
                createdStatus,
                null,
                booking.CreationDate,
                booking.CreatedBy)
        };
        statusChanges.AddRange(changes);

        return BookingResult<BookingHistoryDto>.Success(new BookingHistoryDto(
            booking.Uid,
            booking.BookingNumber,
            booking.Status.ToDatabaseValue(),
            booking.CreationDate,
            booking.CreatedBy,
            statusChanges));
    }
}
