using HMS.Modules.Payments.Application.DTOs;

namespace HMS.Modules.Payments.Application.Abstractions;

public sealed record BookingPaymentContext(
    long Id,
    Guid Uid,
    long OrganizationId,
    long PropertyId,
    Guid PropertyUid,
    long BookingId,
    Guid BookingUid,
    decimal Amount,
    string Status,
    bool IsArchived);

public interface IBookingPaymentRepository
{
    Task<IReadOnlyList<BookingPaymentDto>> GetByBookingIdAsync(long bookingId, CancellationToken cancellationToken);
    Task<Guid> InsertAsync(
        long organizationId,
        long propertyId,
        long bookingId,
        string paymentMethod,
        string paymentType,
        decimal amount,
        string currency,
        string status,
        string? referenceNumber,
        DateTimeOffset paidAt,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken);
    Task<BookingPaymentContext?> GetByUidAsync(Guid paymentUid, CancellationToken cancellationToken);
    Task<decimal> GetRefundedAmountAsync(long paymentId, CancellationToken cancellationToken);
    Task<BookingRefundDto> RefundAsync(
        BookingPaymentContext payment,
        decimal amount,
        string reason,
        string actorSubject,
        CancellationToken cancellationToken);
}
