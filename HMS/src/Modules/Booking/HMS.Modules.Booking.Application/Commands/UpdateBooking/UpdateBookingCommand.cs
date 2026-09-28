using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using HMS.Modules.Booking.Domain.Enums;
using MediatR;

namespace HMS.Modules.Booking.Application.Commands.UpdateBooking;

public sealed record UpdateBookingCommand(
    Guid BookingUid,
    Guid LeadGuestUid,
    BookingSource BookingSource,
    BookingStatus Status,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    int Infants,
    string Currency,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal? QuotedTotal,
    TimeOnly? ArrivalTime,
    TimeOnly? DepartureTime,
    string? SpecialRequests,
    string? InternalNotes,
    string? ExternalReference,
    string? CancellationReason,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDetailDto>>;

public sealed class UpdateBookingCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<UpdateBookingCommand, BookingResult<BookingDetailDto>>
{
    public async Task<BookingResult<BookingDetailDto>> Handle(
        UpdateBookingCommand request,
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
            return BookingResult<BookingDetailDto>.Forbidden("You cannot update this booking.");
        }

        var guest = await bookingRepository.GetGuestAsync(request.LeadGuestUid, cancellationToken);
        if (guest is null)
            return BookingResult<BookingDetailDto>.NotFound("Lead guest was not found.");

        if (guest.OrganizationId != booking.OrganizationId)
            return BookingResult<BookingDetailDto>.Validation(
                "Lead guest does not belong to this booking's organization.");

        try
        {
            booking.Update(
                guest.Id,
                request.LeadGuestUid,
                request.BookingSource,
                request.Status,
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
                request.QuotedTotal,
                request.ArrivalTime,
                request.DepartureTime,
                request.SpecialRequests,
                request.InternalNotes,
                request.CancellationReason,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return BookingResult<BookingDetailDto>.Validation(exception.Message);
        }

        try
        {
            await bookingRepository.UpdateAsync(booking, cancellationToken);
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
