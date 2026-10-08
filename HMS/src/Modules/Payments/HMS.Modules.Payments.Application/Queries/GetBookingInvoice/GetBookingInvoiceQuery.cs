using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Queries.GetBookingInvoice;

public sealed record GetBookingInvoiceQuery(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingInvoiceDto>>;

public sealed class GetBookingInvoiceQueryHandler(
    IPaymentPropertyAccess propertyAccess,
    IPaymentBookingLookup bookingRepository,
    IBookingFinancialRepository financialRepository)
    : IRequestHandler<GetBookingInvoiceQuery, PaymentResult<BookingInvoiceDto>>
{
    public async Task<PaymentResult<BookingInvoiceDto>> Handle(
        GetBookingInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return PaymentResult<BookingInvoiceDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, booking.PropertyUid, false, cancellationToken))
        {
            return PaymentResult<BookingInvoiceDto>.Forbidden("You cannot view the invoice for this booking.");
        }

        var invoice = await financialRepository.GetInvoiceAsync(booking.Id, cancellationToken);
        return invoice is null
            ? PaymentResult<BookingInvoiceDto>.NotFound("Invoice was not found.")
            : PaymentResult<BookingInvoiceDto>.Success(invoice);
    }
}
