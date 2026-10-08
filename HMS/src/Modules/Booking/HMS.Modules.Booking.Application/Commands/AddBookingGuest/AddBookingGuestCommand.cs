using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using HMS.Modules.Booking.Domain.Enums;
using MediatR;

namespace HMS.Modules.Booking.Application.Commands.AddBookingGuest;

public sealed record AddBookingGuestCommand(
    Guid BookingUid,
    Guid GuestUid,
    Guid? BookingUnitUid,
    bool IsLeadGuest,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingGuestDto>>;

public sealed class AddBookingGuestCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<AddBookingGuestCommand, BookingResult<BookingGuestDto>>
{
    public async Task<BookingResult<BookingGuestDto>> Handle(
        AddBookingGuestCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return BookingResult<BookingGuestDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                booking.PropertyUid,
                true,
                cancellationToken))
        {
            return BookingResult<BookingGuestDto>.Forbidden("You cannot add guests to this booking.");
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.CheckedOut
            or BookingStatus.Completed or BookingStatus.NoShow)
        {
            return BookingResult<BookingGuestDto>.Validation(
                "Guests cannot be added to a booking in this status.");
        }

        var guest = await bookingRepository.GetGuestAsync(request.GuestUid, cancellationToken);
        if (guest is null)
            return BookingResult<BookingGuestDto>.NotFound("Guest was not found.");

        if (guest.OrganizationId != booking.OrganizationId)
        {
            return BookingResult<BookingGuestDto>.Validation(
                "The guest does not belong to this booking's organization.");
        }

        long? bookingUnitId = null;
        if (request.BookingUnitUid is Guid bookingUnitUid)
        {
            var bookingUnit = await bookingRepository.GetBookingUnitAsync(
                booking.Id, bookingUnitUid, cancellationToken);
            if (bookingUnit is null)
                return BookingResult<BookingGuestDto>.NotFound("Booking unit line was not found.");

            bookingUnitId = bookingUnit.Id;
        }

        try
        {
            var added = await bookingRepository.AddGuestAsync(
                booking,
                request.GuestUid,
                guest.Id,
                bookingUnitId,
                request.IsLeadGuest,
                request.ActorSubject,
                cancellationToken);

            return BookingResult<BookingGuestDto>.Success(added);
        }
        catch (InvalidOperationException exception)
        {
            return BookingResult<BookingGuestDto>.Conflict(exception.Message);
        }
    }
}
