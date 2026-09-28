using Dapper;
using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.DTOs;

namespace HMS.Modules.Payments.Infrastructure.Persistence.Repositories;

public sealed class BookingPaymentRepository(IPaymentsDbConnectionFactory connectionFactory)
    : IBookingPaymentRepository
{
    public async Task<IReadOnlyList<BookingPaymentDto>> GetByBookingIdAsync(
        long bookingId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.uid               AS "Uid",
                b.uid               AS "BookingUid",
                p.payment_method    AS "PaymentMethod",
                p.payment_type      AS "PaymentType",
                p.amount            AS "Amount",
                rtrim(p.currency)   AS "Currency",
                p.status            AS "Status",
                p.reference_number  AS "ReferenceNumber",
                p.paid_at           AS "PaidAt",
                p.notes             AS "Notes",
                p.creation_date     AS "CreationDate"
            FROM hotel.booking_payments p
            JOIN hotel.bookings b ON b.id = p.booking_id
            WHERE p.booking_id = @BookingId
              AND p.is_archived = false
            ORDER BY p.paid_at DESC, p.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<PaymentRow>(
            new CommandDefinition(sql, new { BookingId = bookingId }, cancellationToken: cancellationToken));
        return rows.Select(ToDto).ToList();
    }

    public async Task<Guid> InsertAsync(
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
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.booking_payments
            (
                uid, organization_id, property_id, booking_id, payment_method, payment_type,
                amount, currency, status, reference_number, paid_at, notes,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @BookingId, @PaymentMethod, @PaymentType,
                @Amount, @Currency, @Status, @ReferenceNumber, @PaidAt, @Notes,
                true, false, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Uid = uid,
                OrganizationId = organizationId,
                PropertyId = propertyId,
                BookingId = bookingId,
                PaymentMethod = paymentMethod,
                PaymentType = paymentType,
                Amount = amount,
                Currency = currency,
                Status = status,
                ReferenceNumber = referenceNumber,
                PaidAt = paidAt,
                Notes = notes,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task<BookingPaymentContext?> GetByUidAsync(Guid paymentUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.id                AS "Id",
                p.uid               AS "Uid",
                p.organization_id   AS "OrganizationId",
                p.property_id       AS "PropertyId",
                pr.uid              AS "PropertyUid",
                p.booking_id        AS "BookingId",
                b.uid               AS "BookingUid",
                p.amount            AS "Amount",
                p.status            AS "Status",
                p.is_archived       AS "IsArchived"
            FROM hotel.booking_payments p
            JOIN hotel.bookings b ON b.id = p.booking_id
            JOIN hotel.properties pr ON pr.id = p.property_id
            WHERE p.uid = @PaymentUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<BookingPaymentContext>(
            new CommandDefinition(sql, new { PaymentUid = paymentUid }, cancellationToken: cancellationToken));
    }

    public async Task<decimal> GetRefundedAmountAsync(long paymentId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COALESCE(SUM(amount), 0)
            FROM hotel.booking_refunds
            WHERE payment_id = @PaymentId
              AND is_archived = false
              AND status IN ('PENDING', 'COMPLETED');
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<decimal>(
            new CommandDefinition(sql, new { PaymentId = paymentId }, cancellationToken: cancellationToken));
    }

    public async Task<BookingRefundDto> RefundAsync(
        BookingPaymentContext payment,
        decimal amount,
        string reason,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        var refundedAt = DateTimeOffset.UtcNow;
        const string sql = """
            INSERT INTO hotel.booking_refunds
            (
                uid, organization_id, property_id, booking_id, payment_id,
                amount, status, reason, refunded_at, is_active, is_archived,
                creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @BookingId, @PaymentId,
                @Amount, 'COMPLETED', @Reason, @RefundedAt, true, false,
                @RefundedAt, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Uid = uid,
                payment.OrganizationId,
                payment.PropertyId,
                payment.BookingId,
                PaymentId = payment.Id,
                Amount = amount,
                Reason = reason,
                RefundedAt = refundedAt,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return new BookingRefundDto(uid, payment.Uid, payment.BookingUid, amount, "COMPLETED", reason, refundedAt);
    }

    private static BookingPaymentDto ToDto(PaymentRow row) => new(
        row.Uid,
        row.BookingUid,
        row.PaymentMethod,
        row.PaymentType,
        row.Amount,
        row.Currency,
        row.Status,
        row.ReferenceNumber,
        ToDateTimeOffset(row.PaidAt),
        row.Notes,
        ToDateTimeOffset(row.CreationDate));

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private sealed class PaymentRow
    {
        public Guid Uid { get; init; }
        public Guid BookingUid { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public string PaymentType { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? ReferenceNumber { get; init; }
        public DateTime PaidAt { get; init; }
        public string? Notes { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
