using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Queries.GetBookingCharges;

public sealed record GetBookingChargesQuery(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<IReadOnlyList<BookingChargeDto>>>;

public sealed class GetBookingChargesQueryHandler(
    IPaymentPropertyAccess propertyAccess,
    IPaymentBookingLookup bookingRepository,
    IBookingChargeRepository chargeRepository)
    : IRequestHandler<GetBookingChargesQuery, PaymentResult<IReadOnlyList<BookingChargeDto>>>
{
    public async Task<PaymentResult<IReadOnlyList<BookingChargeDto>>> Handle(
        GetBookingChargesQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return PaymentResult<IReadOnlyList<BookingChargeDto>>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, booking.PropertyUid, false, cancellationToken))
        {
            return PaymentResult<IReadOnlyList<BookingChargeDto>>.Forbidden(
                "You cannot view charges for this booking.");
        }

        var items = await chargeRepository.GetByBookingIdAsync(booking.Id, cancellationToken);
        return PaymentResult<IReadOnlyList<BookingChargeDto>>.Success(items);
    }
}
