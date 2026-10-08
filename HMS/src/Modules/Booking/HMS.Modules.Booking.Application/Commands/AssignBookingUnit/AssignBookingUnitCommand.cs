using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using HMS.Modules.Booking.Domain.Enums;
using MediatR;

namespace HMS.Modules.Booking.Application.Commands.AssignBookingUnit;

public sealed record AssignBookingUnitCommand(
    Guid BookingUid,
    Guid BookingUnitUid,
    Guid UnitUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDetailDto>>;

public sealed class AssignBookingUnitCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<AssignBookingUnitCommand, BookingResult<BookingDetailDto>>
{
    public async Task<BookingResult<BookingDetailDto>> Handle(
        AssignBookingUnitCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return BookingResult<BookingDetailDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                booking.PropertyUid,
                true,
                cancellationToken))
        {
            return BookingResult<BookingDetailDto>.Forbidden("You cannot modify this booking.");
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.CheckedOut
            or BookingStatus.Completed or BookingStatus.NoShow)
        {
            return BookingResult<BookingDetailDto>.Validation(
                "Units cannot be assigned for a booking in this status.");
        }

        var bookingUnit = await bookingRepository.GetBookingUnitAsync(
            booking.Id, request.BookingUnitUid, cancellationToken);
        if (bookingUnit is null)
            return BookingResult<BookingDetailDto>.NotFound("Booking unit line was not found.");

        if (bookingUnit.AllocationStatus is "RELEASED" or "CANCELLED")
            return BookingResult<BookingDetailDto>.Validation("This booking unit can no longer be reassigned.");

        var unitId = await bookingRepository.GetUnitIdAsync(
            request.UnitUid, booking.PropertyId, bookingUnit.AccommodationTypeId, cancellationToken);
        if (unitId is null)
        {
            return BookingResult<BookingDetailDto>.Validation(
                "The selected unit does not belong to this property and accommodation type.");
        }

        var allocationStatus = booking.Status switch
        {
            BookingStatus.Confirmed => "CONFIRMED",
            BookingStatus.CheckedIn => "CHECKED_IN",
            _ => "HELD"
        };

        try
        {
            await bookingRepository.AssignUnitAsync(
                bookingUnit.Id,
                unitId.Value,
                allocationStatus,
                DateTimeOffset.UtcNow,
                request.ActorSubject,
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BookingResult<BookingDetailDto>.Conflict(exception.Message);
        }

        var detail = await bookingRepository.GetDetailByUidAsync(request.BookingUid, cancellationToken);
        return detail is null
            ? BookingResult<BookingDetailDto>.NotFound("Booking was not found.")
            : BookingResult<BookingDetailDto>.Success(detail);
    }
}