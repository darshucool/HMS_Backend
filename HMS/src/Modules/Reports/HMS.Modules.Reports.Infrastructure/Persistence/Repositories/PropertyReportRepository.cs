using Dapper;
using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.DTOs;

namespace HMS.Modules.Reports.Infrastructure.Persistence.Repositories;

public sealed class PropertyReportRepository(IReportsDbConnectionFactory connectionFactory)
    : IPropertyReportRepository
{
    public async Task<IReadOnlyList<BookingProfitabilityDto>> GetBookingProfitabilityAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                f.booking_uid              AS "BookingUid",
                f.booking_number           AS "BookingNumber",
                f.status                   AS "Status",
                f.booking_source           AS "BookingSource",
                f.check_in_date            AS "CheckInDate",
                f.check_out_date           AS "CheckOutDate",
                f.number_of_nights         AS "Nights",
                f.adults                   AS "Adults",
                f.children                 AS "Children",
                f.infants                  AS "Infants",
                rtrim(f.currency)          AS "Currency",
                f.room_revenue             AS "RoomRevenue",
                f.extra_income             AS "ExtraIncome",
                f.discount_amount          AS "DiscountAmount",
                f.tax_amount               AS "TaxAmount",
                f.service_charge           AS "ServiceCharge",
                f.total_booking_value      AS "TotalBookingValue",
                f.payments_received        AS "PaymentsReceived",
                f.refunds_paid             AS "RefundsPaid",
                f.net_paid                 AS "NetPaid",
                f.outstanding_balance      AS "OutstandingBalance",
                f.direct_expense           AS "DirectExpense",
                f.allocated_overhead       AS "AllocatedOverhead",
                f.estimated_profit         AS "EstimatedProfit",
                f.profit_per_guest_night   AS "ProfitPerGuestNight"
            FROM hotel.vw_booking_profitability f
            JOIN hotel.properties p ON p.id = f.property_id
            WHERE p.uid = @PropertyUid
            ORDER BY f.check_in_date DESC, f.booking_number DESC;
            """;

        return await QueryListAsync<ProfitabilityRow, BookingProfitabilityDto>(
            sql,
            new { PropertyUid = propertyUid },
            row => new BookingProfitabilityDto(
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
                row.OutstandingBalance,
                row.DirectExpense,
                row.AllocatedOverhead,
                row.EstimatedProfit,
                row.ProfitPerGuestNight),
            cancellationToken);
    }

    public async Task<IReadOnlyList<GuestProfitabilityDto>> GetGuestProfitabilityAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                g.uid                                      AS "GuestUid",
                g.display_name                             AS "DisplayName",
                COUNT(*)::int                              AS "BookingCount",
                COALESCE(SUM(f.number_of_nights), 0)::int  AS "Nights",
                COALESCE(SUM(f.total_booking_value), 0)    AS "TotalBookingValue",
                COALESCE(SUM(f.direct_expense), 0)         AS "DirectExpense",
                COALESCE(SUM(f.allocated_overhead), 0)     AS "AllocatedOverhead",
                COALESCE(SUM(f.estimated_profit), 0)       AS "EstimatedProfit"
            FROM hotel.vw_booking_profitability f
            JOIN hotel.bookings b ON b.id = f.booking_id
            JOIN hotel.guests g ON g.id = b.lead_guest_id
            JOIN hotel.properties p ON p.id = f.property_id
            WHERE p.uid = @PropertyUid
            GROUP BY g.uid, g.display_name
            ORDER BY COALESCE(SUM(f.estimated_profit), 0) DESC, g.display_name;
            """;

        return await QueryListAsync<GuestProfitRow, GuestProfitabilityDto>(
            sql,
            new { PropertyUid = propertyUid },
            row => new GuestProfitabilityDto(
                row.GuestUid,
                row.DisplayName,
                row.BookingCount,
                row.Nights,
                row.TotalBookingValue,
                row.DirectExpense,
                row.AllocatedOverhead,
                row.EstimatedProfit),
            cancellationToken);
    }

    public async Task<IReadOnlyList<OccupancyStayDto>> GetOccupancyAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                h.guest_uid             AS "GuestUid",
                h.display_name          AS "DisplayName",
                h.booking_uid           AS "BookingUid",
                h.booking_number        AS "BookingNumber",
                h.status                AS "Status",
                h.check_in_date         AS "CheckInDate",
                h.check_out_date        AS "CheckOutDate",
                h.number_of_nights      AS "Nights",
                h.total_booking_value   AS "TotalBookingValue",
                h.net_paid              AS "NetPaid",
                h.outstanding_balance   AS "OutstandingBalance"
            FROM hotel.vw_guest_booking_history h
            JOIN hotel.properties p ON p.id = h.property_id
            WHERE p.uid = @PropertyUid
            ORDER BY h.check_in_date DESC, h.booking_number DESC, h.display_name;
            """;

        return await QueryListAsync<OccupancyRow, OccupancyStayDto>(
            sql,
            new { PropertyUid = propertyUid },
            row => new OccupancyStayDto(
                row.GuestUid,
                row.DisplayName,
                row.BookingUid,
                row.BookingNumber,
                row.Status,
                row.CheckInDate,
                row.CheckOutDate,
                row.Nights,
                row.TotalBookingValue,
                row.NetPaid,
                row.OutstandingBalance),
            cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethodSummaryDto>> GetPaymentSummaryAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                report_month            AS "ReportMonth",
                payment_method          AS "PaymentMethod",
                rtrim(currency)         AS "Currency",
                payment_count::int      AS "PaymentCount",
                amount_received         AS "AmountReceived"
            FROM hotel.vw_payment_method_summary
            WHERE property_uid = @PropertyUid
              AND report_month = @ReportMonth
            ORDER BY payment_method, currency;
            """;

        return await QueryListAsync<PaymentRow, PaymentMethodSummaryDto>(
            sql,
            new { PropertyUid = propertyUid, ReportMonth = reportMonth },
            row => new PaymentMethodSummaryDto(
                row.ReportMonth,
                row.PaymentMethod,
                row.Currency,
                row.PaymentCount,
                row.AmountReceived),
            cancellationToken);
    }

    public async Task<IReadOnlyList<OutstandingBalanceDto>> GetOutstandingBalancesAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                f.booking_uid           AS "BookingUid",
                f.booking_number        AS "BookingNumber",
                f.status                AS "Status",
                f.check_in_date         AS "CheckInDate",
                f.check_out_date        AS "CheckOutDate",
                rtrim(f.currency)       AS "Currency",
                f.total_booking_value   AS "TotalBookingValue",
                f.net_paid              AS "NetPaid",
                f.outstanding_balance   AS "OutstandingBalance"
            FROM hotel.vw_booking_financial_summary f
            JOIN hotel.properties p ON p.id = f.property_id
            WHERE p.uid = @PropertyUid
              AND f.outstanding_balance > 0
            ORDER BY f.outstanding_balance DESC, f.check_in_date DESC;
            """;

        return await QueryListAsync<OutstandingRow, OutstandingBalanceDto>(
            sql,
            new { PropertyUid = propertyUid },
            row => new OutstandingBalanceDto(
                row.BookingUid,
                row.BookingNumber,
                row.Status,
                row.CheckInDate,
                row.CheckOutDate,
                row.Currency,
                row.TotalBookingValue,
                row.NetPaid,
                row.OutstandingBalance),
            cancellationToken);
    }

    public async Task<ExpenseReportDto?> GetExpensesAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                report_month       AS "ReportMonth",
                staff_expenses     AS "StaffExpenses",
                utility_expenses   AS "UtilityExpenses",
                general_expenses   AS "GeneralExpenses",
                total_expenses     AS "TotalExpenses"
            FROM hotel.vw_monthly_property_summary
            WHERE property_uid = @PropertyUid
              AND report_month = @ReportMonth;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ExpenseRow>(
            new CommandDefinition(
                sql,
                new { PropertyUid = propertyUid, ReportMonth = reportMonth },
                cancellationToken: cancellationToken));

        return row is null
            ? null
            : new ExpenseReportDto(
                row.ReportMonth,
                row.StaffExpenses,
                row.UtilityExpenses,
                row.GeneralExpenses,
                row.TotalExpenses);
    }

    public async Task<IReadOnlyList<UtilityReportDto>> GetUtilitiesAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                report_month      AS "ReportMonth",
                utility_code      AS "UtilityCode",
                utility_name      AS "UtilityName",
                unit_of_measure   AS "UnitOfMeasure",
                total_units       AS "TotalUnits",
                total_cost        AS "TotalCost"
            FROM hotel.vw_monthly_utility_summary
            WHERE property_uid = @PropertyUid
              AND report_month = @ReportMonth
            ORDER BY utility_name;
            """;

        return await QueryListAsync<UtilityRow, UtilityReportDto>(
            sql,
            new { PropertyUid = propertyUid, ReportMonth = reportMonth },
            row => new UtilityReportDto(
                row.ReportMonth,
                row.UtilityCode,
                row.UtilityName,
                row.UnitOfMeasure,
                row.TotalUnits,
                row.TotalCost),
            cancellationToken);
    }

    private async Task<IReadOnlyList<TDto>> QueryListAsync<TRow, TDto>(
        string sql,
        object parameters,
        Func<TRow, TDto> map,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<TRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        return rows.Select(map).ToList();
    }

    private sealed class ProfitabilityRow
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
        public decimal DirectExpense { get; init; }
        public decimal AllocatedOverhead { get; init; }
        public decimal EstimatedProfit { get; init; }
        public decimal? ProfitPerGuestNight { get; init; }
    }

    private sealed class GuestProfitRow
    {
        public Guid GuestUid { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public int BookingCount { get; init; }
        public int Nights { get; init; }
        public decimal TotalBookingValue { get; init; }
        public decimal DirectExpense { get; init; }
        public decimal AllocatedOverhead { get; init; }
        public decimal EstimatedProfit { get; init; }
    }

    private sealed class OccupancyRow
    {
        public Guid GuestUid { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public Guid BookingUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public int Nights { get; init; }
        public decimal TotalBookingValue { get; init; }
        public decimal NetPaid { get; init; }
        public decimal OutstandingBalance { get; init; }
    }

    private sealed class PaymentRow
    {
        public DateOnly ReportMonth { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
        public int PaymentCount { get; init; }
        public decimal AmountReceived { get; init; }
    }

    private sealed class OutstandingRow
    {
        public Guid BookingUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public string Currency { get; init; } = string.Empty;
        public decimal TotalBookingValue { get; init; }
        public decimal NetPaid { get; init; }
        public decimal OutstandingBalance { get; init; }
    }

    private sealed class ExpenseRow
    {
        public DateOnly ReportMonth { get; init; }
        public decimal StaffExpenses { get; init; }
        public decimal UtilityExpenses { get; init; }
        public decimal GeneralExpenses { get; init; }
        public decimal TotalExpenses { get; init; }
    }

    private sealed class UtilityRow
    {
        public DateOnly ReportMonth { get; init; }
        public string UtilityCode { get; init; } = string.Empty;
        public string UtilityName { get; init; } = string.Empty;
        public string? UnitOfMeasure { get; init; }
        public decimal TotalUnits { get; init; }
        public decimal TotalCost { get; init; }
    }
}
