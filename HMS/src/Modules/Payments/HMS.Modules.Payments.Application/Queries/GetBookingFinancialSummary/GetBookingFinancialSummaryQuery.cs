using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Queries.GetBookingFinancialSummary;

public sealed record GetBookingFinancialSummaryQuery(
    Guid BookingUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingFinancialSummaryDto>>;

public sealed class GetBookingFinancialSummaryQueryHandler(
    IPaymentPropertyAccess propertyAccess,
    IPaymentBookingLookup bookingRepository,
    IBookingFinancialRepository financialRepository)
    : IRequestHandler<GetBookingFinancialSummaryQuery, PaymentResult<BookingFinancialSummaryDto>>
{
    public async Task<PaymentResult<BookingFinancialSummaryDto>> Handle(
        GetBookingFinancialSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return PaymentResult<BookingFinancialSummaryDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, booking.PropertyUid, false, cancellationToken))
        {
            return PaymentResult<BookingFinancialSummaryDto>.Forbidden(
                "You cannot view the financial summary for this booking.");
        }

        var summary = await financialRepository.GetSummaryAsync(booking.Id, cancellationToken);
        return summary is null
            ? PaymentResult<BookingFinancialSummaryDto>.NotFound("Financial summary was not found.")
            : PaymentResult<BookingFinancialSummaryDto>.Success(summary);
    }
}
