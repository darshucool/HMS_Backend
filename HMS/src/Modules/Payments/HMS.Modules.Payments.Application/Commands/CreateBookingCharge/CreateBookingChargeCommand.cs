using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.CreateBookingCharge;

public sealed record CreateBookingChargeCommand(
    Guid BookingUid,
    Guid ChargeTypeUid,
    Guid? BookingUnitUid,
    DateOnly ServiceDate,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingChargeDto>>;

public sealed class CreateBookingChargeCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IPaymentBookingLookup bookingRepository,
    IBookingChargeRepository chargeRepository)
    : IRequestHandler<CreateBookingChargeCommand, PaymentResult<BookingChargeDto>>
{
    public async Task<PaymentResult<BookingChargeDto>> Handle(
        CreateBookingChargeCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return PaymentResult<BookingChargeDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, booking.PropertyUid, true, cancellationToken))
        {
            return PaymentResult<BookingChargeDto>.Forbidden("You cannot add charges to this booking.");
        }

        if (booking.Status is "CANCELLED" or "NO_SHOW")
            return PaymentResult<BookingChargeDto>.Validation("Charges cannot be added to this booking.");

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 250)
            return PaymentResult<BookingChargeDto>.Validation("Description is required and cannot exceed 250 characters.");

        if (request.Quantity <= 0 || request.UnitPrice < 0 || request.DiscountAmount < 0 || request.TaxAmount < 0)
            return PaymentResult<BookingChargeDto>.Validation("Quantity must be greater than zero and amounts cannot be negative.");

        var chargeType = await chargeRepository.GetChargeTypeAsync(request.ChargeTypeUid, cancellationToken);
        if (chargeType is null || chargeType.PropertyId != booking.PropertyId)
            return PaymentResult<BookingChargeDto>.NotFound("Charge type was not found for this property.");

        long? bookingUnitId = null;
        if (request.BookingUnitUid is Guid bookingUnitUid)
        {
            var bookingUnit = await bookingRepository.GetBookingUnitAsync(booking.Id, bookingUnitUid, cancellationToken);
            if (bookingUnit is null)
                return PaymentResult<BookingChargeDto>.NotFound("Booking unit line was not found.");

            bookingUnitId = bookingUnit.Id;
        }

        var uid = await chargeRepository.InsertAsync(
            booking.OrganizationId,
            booking.PropertyId,
            booking.Id,
            bookingUnitId,
            chargeType.Id,
            request.ServiceDate,
            request.Description.Trim(),
            request.Quantity,
            request.UnitPrice,
            request.DiscountAmount,
            request.TaxAmount,
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            request.ActorSubject,
            cancellationToken);

        var detail = await chargeRepository.GetDetailByUidAsync(uid, cancellationToken);
        return detail is null
            ? PaymentResult<BookingChargeDto>.NotFound("Charge was not found.")
            : PaymentResult<BookingChargeDto>.Success(detail);
    }
}
