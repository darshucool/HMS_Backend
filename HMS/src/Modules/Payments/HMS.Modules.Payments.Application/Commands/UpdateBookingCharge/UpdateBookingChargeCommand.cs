using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.UpdateBookingCharge;

public sealed record UpdateBookingChargeCommand(
    Guid ChargeUid,
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

public sealed class UpdateBookingChargeCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IPaymentBookingLookup bookingRepository,
    IBookingChargeRepository chargeRepository)
    : IRequestHandler<UpdateBookingChargeCommand, PaymentResult<BookingChargeDto>>
{
    public async Task<PaymentResult<BookingChargeDto>> Handle(
        UpdateBookingChargeCommand request,
        CancellationToken cancellationToken)
    {
        var charge = await chargeRepository.GetByUidAsync(request.ChargeUid, cancellationToken);
        if (charge is null || charge.IsArchived)
            return PaymentResult<BookingChargeDto>.NotFound("Charge was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, charge.PropertyUid, true, cancellationToken))
        {
            return PaymentResult<BookingChargeDto>.Forbidden("You cannot update this charge.");
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 250)
            return PaymentResult<BookingChargeDto>.Validation("Description is required and cannot exceed 250 characters.");

        if (request.Quantity <= 0 || request.UnitPrice < 0 || request.DiscountAmount < 0 || request.TaxAmount < 0)
            return PaymentResult<BookingChargeDto>.Validation("Quantity must be greater than zero and amounts cannot be negative.");

        var chargeType = await chargeRepository.GetChargeTypeAsync(request.ChargeTypeUid, cancellationToken);
        if (chargeType is null || chargeType.PropertyId != charge.PropertyId)
            return PaymentResult<BookingChargeDto>.NotFound("Charge type was not found for this property.");

        long? bookingUnitId = null;
        if (request.BookingUnitUid is Guid bookingUnitUid)
        {
            var bookingUnit = await bookingRepository.GetBookingUnitAsync(charge.BookingId, bookingUnitUid, cancellationToken);
            if (bookingUnit is null)
                return PaymentResult<BookingChargeDto>.NotFound("Booking unit line was not found.");

            bookingUnitId = bookingUnit.Id;
        }

        await chargeRepository.UpdateAsync(
            charge.Id,
            chargeType.Id,
            bookingUnitId,
            request.ServiceDate,
            request.Description.Trim(),
            request.Quantity,
            request.UnitPrice,
            request.DiscountAmount,
            request.TaxAmount,
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            request.ActorSubject,
            cancellationToken);

        var detail = await chargeRepository.GetDetailByUidAsync(request.ChargeUid, cancellationToken);
        return detail is null
            ? PaymentResult<BookingChargeDto>.NotFound("Charge was not found.")
            : PaymentResult<BookingChargeDto>.Success(detail);
    }
}
