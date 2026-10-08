using Dapper;
using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.DTOs;

namespace HMS.Modules.Reports.Infrastructure.Persistence.Repositories;

public sealed class BookingRevenueRepository(IReportsDbConnectionFactory connectionFactory)
    : IBookingRevenueRepository
{
    public async Task<IReadOnlyList<BookingRevenueDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                f.booking_uid           AS "BookingUid",
                f.booking_number        AS "BookingNumber",
                f.status                AS "Status",
                f.booking_source        AS "BookingSource",
                f.check_in_date         AS "CheckInDate",
                f.check_out_date        AS "CheckOutDate",
                f.number_of_nights      AS "Nights",
                f.adults                AS "Adults",
                f.children              AS "Children",
                f.infants               AS "Infants",
                rtrim(f.currency)       AS "Currency",
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
            JOIN hotel.properties p ON p.id = f.property_id
            WHERE p.uid = @PropertyUid
            ORDER BY f.check_in_date DESC, f.booking_number DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<RevenueRow>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));
        return rows.Select(row => new BookingRevenueDto(
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
            row.OutstandingBalance)).ToList();
    }

    private sealed class RevenueRow
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
}
