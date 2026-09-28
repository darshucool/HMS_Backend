using Dapper;
using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.DTOs;

namespace HMS.Modules.Reports.Infrastructure.Persistence.Repositories;

public sealed class ReportPropertyAccessRepository(IReportsDbConnectionFactory connectionFactory)
    : IReportPropertyAccess
{
    public async Task<ReportPropertyContext?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid         AS "Uid",
                name        AS "Name",
                is_archived AS "IsArchived"
            FROM hotel.properties
            WHERE uid = @PropertyUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ReportPropertyContext>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));
    }

    public async Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.app_users u
                JOIN hotel.user_property_access upa ON upa.user_id = u.id
                JOIN hotel.properties p
                  ON p.id = upa.property_id
                 AND p.organization_id = upa.organization_id
                WHERE u.auth_subject = @ActorSubject
                  AND p.uid = @PropertyUid
                  AND u.is_active = true
                  AND u.is_archived = false
                  AND upa.is_active = true
                  AND upa.is_archived = false
                  AND p.is_archived = false
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { ActorSubject = actorSubject, PropertyUid = propertyUid },
            cancellationToken: cancellationToken));
    }
}

public sealed class MonthlyPropertySummaryRepository(IReportsDbConnectionFactory connectionFactory)
    : IMonthlyPropertySummaryRepository
{
    public async Task<MonthlyPropertySummaryDto?> GetAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                property_uid          AS "PropertyUid",
                property_name         AS "PropertyName",
                report_month          AS "ReportMonth",
                booking_revenue       AS "BookingRevenue",
                booking_extra_income  AS "BookingExtraIncome",
                other_income          AS "OtherIncome",
                total_income          AS "TotalIncome",
                staff_expenses        AS "StaffExpenses",
                utility_expenses      AS "UtilityExpenses",
                general_expenses      AS "GeneralExpenses",
                total_expenses        AS "TotalExpenses",
                net_profit            AS "NetProfit"
            FROM hotel.vw_monthly_property_summary
            WHERE property_uid = @PropertyUid
              AND report_month = @ReportMonth;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<SummaryRow>(
            new CommandDefinition(
                sql,
                new { PropertyUid = propertyUid, ReportMonth = reportMonth },
                cancellationToken: cancellationToken));

        return row is null
            ? null
            : new MonthlyPropertySummaryDto(
                row.PropertyUid,
                row.PropertyName,
                row.ReportMonth,
                row.BookingRevenue,
                row.BookingExtraIncome,
                row.OtherIncome,
                row.TotalIncome,
                row.StaffExpenses,
                row.UtilityExpenses,
                row.GeneralExpenses,
                row.TotalExpenses,
                row.NetProfit);
    }

    private sealed class SummaryRow
    {
        public Guid PropertyUid { get; init; }
        public string PropertyName { get; init; } = string.Empty;
        public DateOnly ReportMonth { get; init; }
        public decimal BookingRevenue { get; init; }
        public decimal BookingExtraIncome { get; init; }
        public decimal OtherIncome { get; init; }
        public decimal TotalIncome { get; init; }
        public decimal StaffExpenses { get; init; }
        public decimal UtilityExpenses { get; init; }
        public decimal GeneralExpenses { get; init; }
        public decimal TotalExpenses { get; init; }
        public decimal NetProfit { get; init; }
    }
}
