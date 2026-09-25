using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using MediatR;

namespace HMS.Modules.Booking.Application.Commands.ChangeBookingStatus;

public sealed record ConfirmBookingCommand(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDetailDto>>;

public sealed record CancelBookingCommand(
    Guid BookingUid,
    string Reason,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDetailDto>>;

public sealed record CheckInBookingCommand(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDetailDto>>;

public sealed record CheckOutBookingCommand(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDetailDto>>;


public sealed class ConfirmBookingCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<ConfirmBookingCommand, BookingResult<BookingDetailDto>>
{
    public async Task<BookingResult<BookingDetailDto>> Handle(
        ConfirmBookingCommand request,
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
            return BookingResult<BookingDetailDto>.Forbidden("You cannot confirm this booking.");
        }

        try
        {
            booking.Confirm(request.ActorSubject);
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

public sealed class CancelBookingCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<CancelBookingCommand, BookingResult<BookingDetailDto>>
{
    public async Task<BookingResult<BookingDetailDto>> Handle(
        CancelBookingCommand request,
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
            return BookingResult<BookingDetailDto>.Forbidden("You cannot cancel this booking.");
        }

        try
        {
            booking.Cancel(request.Reason, request.ActorSubject);
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
public sealed class CheckInBookingCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<CheckInBookingCommand, BookingResult<BookingDetailDto>>
{
    public async Task<BookingResult<BookingDetailDto>> Handle(
        CheckInBookingCommand request,
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
            return BookingResult<BookingDetailDto>.Forbidden("You cannot check in this booking.");
        }

        try
        {
            booking.CheckIn(request.ActorSubject);
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

public sealed class CheckOutBookingCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<CheckOutBookingCommand, BookingResult<BookingDetailDto>>
{
    public async Task<BookingResult<BookingDetailDto>> Handle(
        CheckOutBookingCommand request,
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
            return BookingResult<BookingDetailDto>.Forbidden("You cannot check out this booking.");
        }

        try
        {
            booking.CheckOut(request.ActorSubject);
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

