using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using HMS.Modules.Payments.Domain.Enums;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.CreateBookingPayment;

public sealed record CreateBookingPaymentCommand(
    Guid BookingUid,
    BookingPaymentMethod PaymentMethod,
    BookingPaymentType PaymentType,
    decimal Amount,
    string Currency,
    BookingPaymentStatus Status,
    string? ReferenceNumber,
    DateTimeOffset? PaidAt,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingPaymentDto>>;

public sealed class CreateBookingPaymentCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IPaymentBookingLookup bookingRepository,
    IBookingPaymentRepository paymentRepository)
    : IRequestHandler<CreateBookingPaymentCommand, PaymentResult<BookingPaymentDto>>
{
    public async Task<PaymentResult<BookingPaymentDto>> Handle(
        CreateBookingPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByUidAsync(request.BookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return PaymentResult<BookingPaymentDto>.NotFound("Booking was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, booking.PropertyUid, true, cancellationToken))
        {
            return PaymentResult<BookingPaymentDto>.Forbidden("You cannot add payments to this booking.");
        }

        if (booking.Status is "CANCELLED" or "NO_SHOW")
            return PaymentResult<BookingPaymentDto>.Validation("Payments cannot be added to this booking.");

        if (request.Amount <= 0)
            return PaymentResult<BookingPaymentDto>.Validation("Amount must be greater than zero.");

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "LKR" : request.Currency.Trim().ToUpperInvariant();
        if (currency.Length != 3)
            return PaymentResult<BookingPaymentDto>.Validation("Currency must be a 3-letter code.");

        if (request.ReferenceNumber is { Length: > 150 })
            return PaymentResult<BookingPaymentDto>.Validation("Reference number cannot exceed 150 characters.");

        var uid = await paymentRepository.InsertAsync(
            booking.OrganizationId,
            booking.PropertyId,
            booking.Id,
            request.PaymentMethod.ToDatabaseValue(),
            request.PaymentType.ToDatabaseValue(),
            request.Amount,
            currency,
            request.Status.ToDatabaseValue(),
            string.IsNullOrWhiteSpace(request.ReferenceNumber) ? null : request.ReferenceNumber.Trim(),
            request.PaidAt ?? DateTimeOffset.UtcNow,
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            request.ActorSubject,
            cancellationToken);

        var items = await paymentRepository.GetByBookingIdAsync(booking.Id, cancellationToken);
        var created = items.FirstOrDefault(item => item.Uid == uid);
        return created is null
            ? PaymentResult<BookingPaymentDto>.NotFound("Payment was not found.")
            : PaymentResult<BookingPaymentDto>.Success(created);
    }
}
