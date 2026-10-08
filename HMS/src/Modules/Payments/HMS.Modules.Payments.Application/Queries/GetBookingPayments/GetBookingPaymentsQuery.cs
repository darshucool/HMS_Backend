using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Queries.GetBookingPayments;

public sealed record GetBookingPaymentsQuery(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<IReadOnlyList<BookingPaymentDto>>>;

public sealed class GetBookingPaymentsQueryHandler(
    IPaymentPropertyAccess propertyAccess,
    IPaymentBookingLookup bookingRepository,
    IBookingPaymentRepository paymentRepository)
    : IRequestHandler<GetBookingPaymentsQuery, PaymentResult<IReadOnlyList<BookingPaymentDto>>>
{
    public async Task<PaymentResult<IReadOnlyList<BookingPaymentDto>>> Handle(
        GetBookingPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return PaymentResult<IReadOnlyList<BookingPaymentDto>>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, booking.PropertyUid, false, cancellationToken))
        {
            return PaymentResult<IReadOnlyList<BookingPaymentDto>>.Forbidden(
                "You cannot view payments for this booking.");
        }

        var items = await paymentRepository.GetByBookingIdAsync(booking.Id, cancellationToken);
        return PaymentResult<IReadOnlyList<BookingPaymentDto>>.Success(items);
    }
}
