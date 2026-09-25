using Dapper;
using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.DTOs;
using UtilityBillEntity = HMS.Modules.Finance.Domain.Entities.UtilityBill;

namespace HMS.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class UtilityBillRepository(IFinanceDbConnectionFactory connectionFactory)
    : IUtilityBillRepository
{
    public async Task<UtilityTypeRef?> GetTypeAsync(
        Guid utilityTypeUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                uid             AS "Uid",
                name            AS "Name",
                unit_of_measure AS "UnitOfMeasure",
                organization_id AS "OrganizationId"
            FROM hotel.utility_types
            WHERE uid = @UtilityTypeUid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UtilityTypeRef>(
            new CommandDefinition(sql, new { UtilityTypeUid = utilityTypeUid }, cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(UtilityBillEntity bill, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.utility_bills
            (
                uid, organization_id, property_id, utility_type_id, period_start, period_end,
                previous_reading, current_reading, units_used, amount, currency, due_date,
                paid_at, reference_number, notes, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @UtilityTypeId, @PeriodStart, @PeriodEnd,
                @PreviousReading, @CurrentReading, @UnitsUsed, @Amount, @Currency, @DueDate,
                @PaidAt, @ReferenceNumber, @Notes, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                bill.Uid,
                bill.OrganizationId,
                bill.PropertyId,
                bill.UtilityTypeId,
                bill.PeriodStart,
                bill.PeriodEnd,
                bill.PreviousReading,
                bill.CurrentReading,
                bill.UnitsUsed,
                bill.Amount,
                bill.Currency,
                bill.DueDate,
                bill.PaidAt,
                bill.ReferenceNumber,
                bill.Notes,
                bill.IsActive,
                bill.IsArchived,
                bill.CreationDate,
                bill.CreatedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<UtilityBillEntity?> GetByUidAsync(Guid utilityBillUid, CancellationToken cancellationToken)
{
    const string sql = """
        SELECT
            b.id                    AS "Id",
            b.uid                   AS "Uid",
            b.organization_id       AS "OrganizationId",
            b.property_id           AS "PropertyId",
            p.uid                   AS "PropertyUid",
            b.utility_type_id       AS "UtilityTypeId",
            t.uid                   AS "UtilityTypeUid",
            b.period_start          AS "PeriodStart",
            b.period_end            AS "PeriodEnd",
            b.previous_reading      AS "PreviousReading",
            b.current_reading       AS "CurrentReading",
            b.units_used            AS "UnitsUsed",
            b.amount                AS "Amount",
            rtrim(b.currency)       AS "Currency",
            b.due_date              AS "DueDate",
            b.paid_at               AS "PaidAt",
            b.reference_number      AS "ReferenceNumber",
            b.notes                 AS "Notes",
            b.is_active             AS "IsActive",
            b.is_archived           AS "IsArchived",
            b.creation_date         AS "CreationDate",
            b.created_by            AS "CreatedBy",
            b.modified_date         AS "ModifiedDate",
            b.modified_by           AS "ModifiedBy"
        FROM hotel.utility_bills b
        JOIN hotel.properties p ON p.id = b.property_id
        JOIN hotel.utility_types t ON t.id = b.utility_type_id
        WHERE b.uid = @UtilityBillUid;
        """;

    await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
    var row = await connection.QuerySingleOrDefaultAsync<UtilityBillHeaderRow>(
        new CommandDefinition(sql, new { UtilityBillUid = utilityBillUid }, cancellationToken: cancellationToken));

    return row is null ? null : ToDomain(row);
}

public async Task<UtilityBillDto?> GetDetailByUidAsync(Guid utilityBillUid, CancellationToken cancellationToken)
{
    const string sql = """
        SELECT
            b.uid                   AS "Uid",
            p.uid                   AS "PropertyUid",
            t.uid                   AS "UtilityTypeUid",
            t.name                  AS "UtilityTypeName",
            t.unit_of_measure       AS "UnitOfMeasure",
            b.period_start          AS "PeriodStart",
            b.period_end            AS "PeriodEnd",
            b.previous_reading      AS "PreviousReading",
            b.current_reading       AS "CurrentReading",
            b.units_used            AS "UnitsUsed",
            b.amount                AS "Amount",
            rtrim(b.currency)       AS "Currency",
            b.due_date              AS "DueDate",
            b.paid_at               AS "PaidAt",
            b.reference_number      AS "ReferenceNumber",
            b.notes                 AS "Notes",
            b.creation_date         AS "CreationDate"
        FROM hotel.utility_bills b
        JOIN hotel.properties p ON p.id = b.property_id
        JOIN hotel.utility_types t ON t.id = b.utility_type_id
        WHERE b.uid = @UtilityBillUid
          AND b.is_archived = false;
        """;

    await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
    var row = await connection.QuerySingleOrDefaultAsync<UtilityBillListRow>(
        new CommandDefinition(sql, new { UtilityBillUid = utilityBillUid }, cancellationToken: cancellationToken));

    return row is null ? null : new UtilityBillDto(
        row.Uid, row.PropertyUid, row.UtilityTypeUid, row.UtilityTypeName, row.UnitOfMeasure,
        row.PeriodStart, row.PeriodEnd, row.PreviousReading, row.CurrentReading, row.UnitsUsed,
        row.Amount, row.Currency, row.DueDate, ToDateTimeOffset(row.PaidAt),
        row.ReferenceNumber, row.Notes, ToDateTimeOffset(row.CreationDate));
}

public async Task UpdateAsync(UtilityBillEntity bill, CancellationToken cancellationToken)
{
    const string sql = """
        UPDATE hotel.utility_bills
        SET utility_type_id = @UtilityTypeId,
            period_start = @PeriodStart,
            period_end = @PeriodEnd,
            previous_reading = @PreviousReading,
            current_reading = @CurrentReading,
            units_used = @UnitsUsed,
            amount = @Amount,
            currency = @Currency,
            due_date = @DueDate,
            paid_at = @PaidAt,
            reference_number = @ReferenceNumber,
            notes = @Notes,
            modified_date = @ModifiedDate,
            modified_by = @ModifiedBy
        WHERE uid = @Uid
          AND is_archived = false;
        """;

    await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
    await connection.ExecuteAsync(new CommandDefinition(
        sql,
        new
        {
            bill.Uid,
            bill.UtilityTypeId,
            bill.PeriodStart,
            bill.PeriodEnd,
            bill.PreviousReading,
            bill.CurrentReading,
            bill.UnitsUsed,
            bill.Amount,
            bill.Currency,
            bill.DueDate,
            bill.PaidAt,
            bill.ReferenceNumber,
            bill.Notes,
            bill.ModifiedDate,
            bill.ModifiedBy
        },
        cancellationToken: cancellationToken));
}

private static UtilityBillEntity ToDomain(UtilityBillHeaderRow row) => UtilityBillEntity.Rehydrate(
    row.Id, row.Uid, row.OrganizationId, row.PropertyId, row.PropertyUid,
    row.UtilityTypeId, row.UtilityTypeUid, row.PeriodStart, row.PeriodEnd,
    row.PreviousReading, row.CurrentReading, row.UnitsUsed, row.Amount, row.Currency,
    row.DueDate, ToDateTimeOffset(row.PaidAt), row.ReferenceNumber, row.Notes,
    row.IsActive, row.IsArchived, ToDateTimeOffset(row.CreationDate), row.CreatedBy,
    ToDateTimeOffset(row.ModifiedDate), row.ModifiedBy);

private sealed class UtilityBillHeaderRow
{
    public long Id { get; init; }
    public Guid Uid { get; init; }
    public long OrganizationId { get; init; }
    public long PropertyId { get; init; }
    public Guid PropertyUid { get; init; }
    public long UtilityTypeId { get; init; }
    public Guid UtilityTypeUid { get; init; }
    public DateOnly PeriodStart { get; init; }
    public DateOnly PeriodEnd { get; init; }
    public decimal? PreviousReading { get; init; }
    public decimal? CurrentReading { get; init; }
    public decimal? UnitsUsed { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateOnly? DueDate { get; init; }
    public DateTime? PaidAt { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreationDate { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedDate { get; init; }
    public string? ModifiedBy { get; init; }
}

    public async Task<IReadOnlyList<UtilityBillDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                b.uid                   AS "Uid",
                p.uid                   AS "PropertyUid",
                t.uid                   AS "UtilityTypeUid",
                t.name                  AS "UtilityTypeName",
                t.unit_of_measure       AS "UnitOfMeasure",
                b.period_start          AS "PeriodStart",
                b.period_end            AS "PeriodEnd",
                b.previous_reading      AS "PreviousReading",
                b.current_reading       AS "CurrentReading",
                b.units_used            AS "UnitsUsed",
                b.amount                AS "Amount",
                rtrim(b.currency)       AS "Currency",
                b.due_date              AS "DueDate",
                b.paid_at               AS "PaidAt",
                b.reference_number      AS "ReferenceNumber",
                b.notes                 AS "Notes",
                b.creation_date         AS "CreationDate"
            FROM hotel.utility_bills b
            JOIN hotel.properties p ON p.id = b.property_id
            JOIN hotel.utility_types t ON t.id = b.utility_type_id
            WHERE p.uid = @PropertyUid
              AND b.is_archived = false
            ORDER BY b.period_start DESC, b.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<UtilityBillListRow>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));

        return rows.Select(row => new UtilityBillDto(
            row.Uid,
            row.PropertyUid,
            row.UtilityTypeUid,
            row.UtilityTypeName,
            row.UnitOfMeasure,
            row.PeriodStart,
            row.PeriodEnd,
            row.PreviousReading,
            row.CurrentReading,
            row.UnitsUsed,
            row.Amount,
            row.Currency,
            row.DueDate,
            ToDateTimeOffset(row.PaidAt),
            row.ReferenceNumber,
            row.Notes,
            ToDateTimeOffset(row.CreationDate))).ToList();
    }

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value) =>
        value is null ? null : ToDateTimeOffset(value.Value);

    private sealed class UtilityBillListRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public Guid UtilityTypeUid { get; init; }
        public string UtilityTypeName { get; init; } = string.Empty;
        public string? UnitOfMeasure { get; init; }
        public DateOnly PeriodStart { get; init; }
        public DateOnly PeriodEnd { get; init; }
        public decimal? PreviousReading { get; init; }
        public decimal? CurrentReading { get; init; }
        public decimal? UnitsUsed { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public DateOnly? DueDate { get; init; }
        public DateTime? PaidAt { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? Notes { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
