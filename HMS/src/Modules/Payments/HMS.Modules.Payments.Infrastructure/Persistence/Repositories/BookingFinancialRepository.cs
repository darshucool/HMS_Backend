using Dapper;
using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.DTOs;

namespace HMS.Modules.Payments.Infrastructure.Persistence.Repositories;

public sealed class BookingFinancialRepository(IPaymentsDbConnectionFactory connectionFactory)
    : IBookingFinancialRepository
{
    public async Task<BookingFinancialSummaryDto?> GetSummaryAsync(
        long bookingId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                booking_uid         AS "BookingUid",
                booking_number      AS "BookingNumber",
                status              AS "Status",
                booking_source      AS "BookingSource",
                check_in_date       AS "CheckInDate",
                check_out_date      AS "CheckOutDate",
                number_of_nights    AS "Nights",
                adults              AS "Adults",
                children            AS "Children",
                infants             AS "Infants",
                rtrim(currency)     AS "Currency",
                room_revenue        AS "RoomRevenue",
                extra_income        AS "ExtraIncome",
                discount_amount     AS "DiscountAmount",
                tax_amount          AS "TaxAmount",
                service_charge      AS "ServiceCharge",
                total_booking_value AS "TotalBookingValue",
                payments_received   AS "PaymentsReceived",
                refunds_paid        AS "RefundsPaid",
                net_paid            AS "NetPaid",
                outstanding_balance AS "OutstandingBalance"
            FROM hotel.vw_booking_financial_summary
            WHERE booking_id = @BookingId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<SummaryRow>(
            new CommandDefinition(sql, new { BookingId = bookingId }, cancellationToken: cancellationToken));
        return row is null ? null : ToSummary(row);
    }

    private static BookingFinancialSummaryDto ToSummary(SummaryRow row) => new(
        row.BookingUid,
        row.BookingNumber,
        row.Status,
        row.BookingSource,
        row.CheckInDate,
        row.CheckOutDate,
        row.Nights,
        row.Adults,
        row.Children,
        row.Infants,
        row.Currency,
        row.RoomRevenue,
        row.ExtraIncome,
        row.DiscountAmount,
        row.TaxAmount,
        row.ServiceCharge,
        row.TotalBookingValue,
        row.PaymentsReceived,
        row.RefundsPaid,
        row.NetPaid,
        row.OutstandingBalance);

    private sealed class SummaryRow
    {
        public Guid BookingUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string BookingSource { get; init; } = string.Empty;
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public int Nights { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public int Infants { get; init; }
        public string Currency { get; init; } = string.Empty;
        public decimal RoomRevenue { get; init; }
        public decimal ExtraIncome { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal ServiceCharge { get; init; }
        public decimal TotalBookingValue { get; init; }
        public decimal PaymentsReceived { get; init; }
        public decimal RefundsPaid { get; init; }
        public decimal NetPaid { get; init; }
        public decimal OutstandingBalance { get; init; }
    }

    public async Task<BookingInvoiceDto?> GetInvoiceAsync(long bookingId, CancellationToken cancellationToken)
    {
        const string headerSql = """
            SELECT
                f.booking_uid           AS "BookingUid",
                f.booking_number        AS "BookingNumber",
                f.status                AS "Status",
                f.check_in_date         AS "CheckInDate",
                f.check_out_date        AS "CheckOutDate",
                f.number_of_nights      AS "Nights",
                f.adults                AS "Adults",
                f.children              AS "Children",
                f.infants               AS "Infants",
                rtrim(f.currency)       AS "Currency",
                b.creation_date         AS "CreationDate",
                p.name                  AS "PropertyName",
                p.address_line1         AS "AddressLine1",
                p.address_line2         AS "AddressLine2",
                p.city                  AS "City",
                p.postal_code           AS "PostalCode",
                p.phone                 AS "PropertyPhone",
                p.email                 AS "PropertyEmail",
                COALESCE(ps.invoice_number_prefix, 'INV') AS "InvoicePrefix",
                g.display_name          AS "GuestName",
                g.phone                 AS "GuestPhone",
                g.email                 AS "GuestEmail",
                g.address               AS "GuestAddress",
                g.city                  AS "GuestCity",
                f.room_revenue          AS "RoomRevenue",
                f.extra_income          AS "ExtraIncome",
                f.discount_amount       AS "DiscountAmount",
                f.tax_amount            AS "TaxAmount",
                f.service_charge        AS "ServiceCharge",
                f.total_booking_value   AS "TotalBookingValue",
                f.payments_received     AS "PaymentsReceived",
                f.refunds_paid          AS "RefundsPaid",
                f.net_paid              AS "NetPaid",
                f.outstanding_balance   AS "OutstandingBalance"
            FROM hotel.vw_booking_financial_summary f
            JOIN hotel.bookings b ON b.id = f.booking_id
            JOIN hotel.properties p ON p.id = f.property_id
            LEFT JOIN hotel.property_settings ps
                ON ps.property_id = f.property_id
               AND ps.is_archived = false
            LEFT JOIN hotel.guests g ON g.id = b.lead_guest_id
            WHERE f.booking_id = @BookingId;
            """;

        const string roomSql = """
            SELECT
                t.name              AS "AccommodationName",
                u.unit_code         AS "UnitCode",
                u.unit_name         AS "UnitName",
                bu.unit_quantity    AS "Quantity",
                bu.number_of_nights AS "Nights",
                bu.unit_rate        AS "UnitPrice",
                bu.discount_amount  AS "DiscountAmount",
                bu.tax_amount       AS "TaxAmount",
                bu.total_amount     AS "TotalAmount"
            FROM hotel.booking_units bu
            JOIN hotel.accommodation_types t ON t.id = bu.accommodation_type_id
            LEFT JOIN hotel.accommodation_units u ON u.id = bu.unit_id
            WHERE bu.booking_id = @BookingId
              AND bu.is_active = true
              AND bu.is_archived = false
              AND bu.allocation_status <> 'CANCELLED'
            ORDER BY bu.check_in_date, bu.id;
            """;

        const string chargeSql = """
            SELECT
                c.description       AS "Description",
                ct.name             AS "ChargeTypeName",
                c.quantity          AS "Quantity",
                c.unit_price        AS "UnitPrice",
                c.discount_amount   AS "DiscountAmount",
                c.tax_amount        AS "TaxAmount",
                c.total_amount      AS "TotalAmount"
            FROM hotel.booking_charges c
            JOIN hotel.booking_charge_types ct ON ct.id = c.charge_type_id
            WHERE c.booking_id = @BookingId
              AND c.is_active = true
              AND c.is_archived = false
            ORDER BY c.service_date, c.id;
            """;

        const string paymentSql = """
            SELECT
                uid                 AS "Uid",
                paid_at             AS "PaidAt",
                payment_method      AS "PaymentMethod",
                payment_type        AS "PaymentType",
                amount              AS "Amount",
                status              AS "Status",
                reference_number    AS "ReferenceNumber"
            FROM hotel.booking_payments
            WHERE booking_id = @BookingId
              AND is_active = true
              AND is_archived = false
            ORDER BY paid_at, id;
            """;

        const string refundSql = """
            SELECT
                r.uid           AS "Uid",
                p.uid           AS "PaymentUid",
                r.refunded_at   AS "RefundedAt",
                r.amount        AS "Amount",
                r.status        AS "Status",
                r.reason        AS "Reason"
            FROM hotel.booking_refunds r
            JOIN hotel.booking_payments p ON p.id = r.payment_id
            WHERE r.booking_id = @BookingId
              AND r.is_active = true
              AND r.is_archived = false
            ORDER BY r.refunded_at, r.id;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var header = await connection.QuerySingleOrDefaultAsync<InvoiceHeaderRow>(
            new CommandDefinition(headerSql, new { BookingId = bookingId }, cancellationToken: cancellationToken));
        if (header is null)
            return null;

        var rooms = (await connection.QueryAsync<RoomLineRow>(
            new CommandDefinition(roomSql, new { BookingId = bookingId }, cancellationToken: cancellationToken))).ToList();
        var charges = (await connection.QueryAsync<ChargeLineRow>(
            new CommandDefinition(chargeSql, new { BookingId = bookingId }, cancellationToken: cancellationToken))).ToList();
        var payments = (await connection.QueryAsync<PaymentLineRow>(
            new CommandDefinition(paymentSql, new { BookingId = bookingId }, cancellationToken: cancellationToken))).ToList();
        var refunds = (await connection.QueryAsync<RefundLineRow>(
            new CommandDefinition(refundSql, new { BookingId = bookingId }, cancellationToken: cancellationToken))).ToList();

        var lines = rooms.Select(room => new BookingInvoiceLineDto(
                "ROOM",
                RoomDescription(room),
                room.Quantity,
                room.UnitPrice,
                room.DiscountAmount,
                room.TaxAmount,
                room.TotalAmount))
            .Concat(charges.Select(charge => new BookingInvoiceLineDto(
                "CHARGE",
                string.IsNullOrWhiteSpace(charge.ChargeTypeName)
                    ? charge.Description
                    : $"{charge.ChargeTypeName}: {charge.Description}",
                charge.Quantity,
                charge.UnitPrice,
                charge.DiscountAmount,
                charge.TaxAmount,
                charge.TotalAmount)))
            .ToList();

        return new BookingInvoiceDto(
            $"{header.InvoicePrefix.Trim()}-{header.BookingNumber}",
            DateOnly.FromDateTime(ToDateTimeOffset(header.CreationDate).UtcDateTime),
            header.BookingUid,
            header.BookingNumber,
            header.Status,
            header.CheckInDate,
            header.CheckOutDate,
            header.Nights,
            header.Adults,
            header.Children,
            header.Infants,
            header.Currency,
            new BookingInvoicePartyDto(
                header.PropertyName,
                header.PropertyPhone,
                header.PropertyEmail,
                JoinAddress(header.AddressLine1, header.AddressLine2, header.City, header.PostalCode)),
            string.IsNullOrWhiteSpace(header.GuestName)
                ? null
                : new BookingInvoicePartyDto(
                    header.GuestName,
                    header.GuestPhone,
                    header.GuestEmail,
                    JoinAddress(header.GuestAddress, header.GuestCity)),
            lines,
            header.RoomRevenue,
            header.ExtraIncome,
            header.DiscountAmount,
            header.TaxAmount,
            header.ServiceCharge,
            header.TotalBookingValue,
            payments.Select(payment => new BookingInvoicePaymentDto(
                payment.Uid,
                ToDateTimeOffset(payment.PaidAt),
                payment.PaymentMethod,
                payment.PaymentType,
                payment.Amount,
                payment.Status,
                payment.ReferenceNumber)).ToList(),
            refunds.Select(refund => new BookingInvoiceRefundDto(
                refund.Uid,
                refund.PaymentUid,
                ToDateTimeOffset(refund.RefundedAt),
                refund.Amount,
                refund.Status,
                refund.Reason)).ToList(),
            header.PaymentsReceived,
            header.RefundsPaid,
            header.NetPaid,
            header.OutstandingBalance);
    }

    private static string RoomDescription(RoomLineRow room)
    {
        var unit = !string.IsNullOrWhiteSpace(room.UnitName)
            ? room.UnitName
            : room.UnitCode;
        var stay = room.Nights == 1 ? "1 night" : $"{room.Nights} nights";
        return string.IsNullOrWhiteSpace(unit)
            ? $"{room.AccommodationName} · {stay}"
            : $"{room.AccommodationName} ({unit}) · {stay}";
    }

    private static string? JoinAddress(params string?[] parts)
    {
        var address = string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.Trim()));
        return string.IsNullOrWhiteSpace(address) ? null : address;
    }

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private sealed class InvoiceHeaderRow
    {
        public Guid BookingUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public int Nights { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public int Infants { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateTime CreationDate { get; init; }
        public string PropertyName { get; init; } = string.Empty;
        public string? AddressLine1 { get; init; }
        public string? AddressLine2 { get; init; }
        public string? City { get; init; }
        public string? PostalCode { get; init; }
        public string? PropertyPhone { get; init; }
        public string? PropertyEmail { get; init; }
        public string InvoicePrefix { get; init; } = "INV";
        public string? GuestName { get; init; }
        public string? GuestPhone { get; init; }
        public string? GuestEmail { get; init; }
        public string? GuestAddress { get; init; }
        public string? GuestCity { get; init; }
        public decimal RoomRevenue { get; init; }
        public decimal ExtraIncome { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal ServiceCharge { get; init; }
        public decimal TotalBookingValue { get; init; }
        public decimal PaymentsReceived { get; init; }
        public decimal RefundsPaid { get; init; }
        public decimal NetPaid { get; init; }
        public decimal OutstandingBalance { get; init; }
    }

    private sealed class RoomLineRow
    {
        public string AccommodationName { get; init; } = string.Empty;
        public string? UnitCode { get; init; }
        public string? UnitName { get; init; }
        public int Quantity { get; init; }
        public int Nights { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
    }

    private sealed class ChargeLineRow
    {
        public string Description { get; init; } = string.Empty;
        public string ChargeTypeName { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
    }

    private sealed class PaymentLineRow
    {
        public Guid Uid { get; init; }
        public DateTime PaidAt { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public string PaymentType { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Status { get; init; } = string.Empty;
        public string? ReferenceNumber { get; init; }
    }

    private sealed class RefundLineRow
    {
        public Guid Uid { get; init; }
        public Guid PaymentUid { get; init; }
        public DateTime RefundedAt { get; init; }
        public decimal Amount { get; init; }
        public string Status { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
    }
}
