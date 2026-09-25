using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Domain.Enums;
using MediatR;

namespace HMS.Modules.Booking.Application.Commands.RemoveBookingGuest;

public sealed record RemoveBookingGuestCommand(
    Guid BookingUid,
    Guid GuestUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<bool>>;

public sealed class RemoveBookingGuestCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<RemoveBookingGuestCommand, BookingResult<bool>>
{
    public async Task<BookingResult<bool>> Handle(
        RemoveBookingGuestCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return BookingResult<bool>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                booking.PropertyUid,
                true,
                cancellationToken))
        {
            return BookingResult<bool>.Forbidden("You cannot remove guests from this booking.");
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.CheckedOut
            or BookingStatus.Completed or BookingStatus.NoShow)
        {
            return BookingResult<bool>.Validation(
                "Guests cannot be removed from a booking in this status.");
        }

        var guest = await bookingRepository.GetGuestAsync(request.GuestUid, cancellationToken);
        if (guest is null || guest.OrganizationId != booking.OrganizationId)
            return BookingResult<bool>.NotFound("Guest was not found.");

        var bookingGuest = await bookingRepository.GetBookingGuestAsync(
            booking.Id, guest.Id, cancellationToken);
        if (bookingGuest is null)
            return BookingResult<bool>.NotFound("This guest is not part of the booking.");

        if (bookingGuest.IsLeadGuest)
        {
            return BookingResult<bool>.Validation(
                "The lead guest cannot be removed. Assign a new lead guest first.");
        }

        try
        {
            await bookingRepository.RemoveGuestAsync(
                bookingGuest.Id,
                request.ActorSubject,
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BookingResult<bool>.Validation(exception.Message);
        }

        return BookingResult<bool>.Success(true);
    }
}