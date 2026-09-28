using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.RefundBookingPayment;

public sealed record RefundBookingPaymentCommand(
    Guid PaymentUid,
    decimal Amount,
    string Reason,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingRefundDto>>;

public sealed class RefundBookingPaymentCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IBookingPaymentRepository paymentRepository)
    : IRequestHandler<RefundBookingPaymentCommand, PaymentResult<BookingRefundDto>>
{
    public async Task<PaymentResult<BookingRefundDto>> Handle(
        RefundBookingPaymentCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return PaymentResult<BookingRefundDto>.Validation("Refund amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return PaymentResult<BookingRefundDto>.Validation("A refund reason is required.");

        var payment = await paymentRepository.GetByUidAsync(request.PaymentUid, cancellationToken);
        if (payment is null || payment.IsArchived)
            return PaymentResult<BookingRefundDto>.NotFound("Payment was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, payment.PropertyUid, true, cancellationToken))
        {
            return PaymentResult<BookingRefundDto>.Forbidden("You cannot refund this payment.");
        }

        if (!string.Equals(payment.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
            return PaymentResult<BookingRefundDto>.Validation("Only a completed payment can be refunded.");

        var alreadyRefunded = await paymentRepository.GetRefundedAmountAsync(payment.Id, cancellationToken);
        if (request.Amount > payment.Amount - alreadyRefunded)
            return PaymentResult<BookingRefundDto>.Validation("Refund amount exceeds the remaining payment balance.");

        var refund = await paymentRepository.RefundAsync(
            payment,
            request.Amount,
            request.Reason.Trim(),
            request.ActorSubject,
            cancellationToken);

        return PaymentResult<BookingRefundDto>.Success(refund);
    }
}
