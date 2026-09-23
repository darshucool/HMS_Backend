using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class UnitBlockRepository(IHotelsDbConnectionFactory connectionFactory)
    : IUnitBlockRepository
{
    public async Task<UnitBlock?> GetByUidAsync(Guid uid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                b.id                    AS "Id",
                b.uid                   AS "Uid",
                b.organization_id       AS "OrganizationId",
                b.property_id           AS "PropertyId",
                p.uid                   AS "PropertyUid",
                b.unit_id               AS "UnitId",
                u.uid                   AS "UnitUid",
                b.start_date            AS "StartDate",
                b.end_date              AS "EndDate",
                b.block_type            AS "BlockType",
                b.reason                AS "Reason",
                b.is_active             AS "IsActive",
                b.is_archived           AS "IsArchived",
                b.creation_date         AS "CreationDate",
                b.created_by            AS "CreatedBy",
                b.modified_date         AS "ModifiedDate",
                b.modified_by           AS "ModifiedBy"
            FROM hotel.unit_blocks b
            JOIN hotel.properties p ON p.id = b.property_id
            JOIN hotel.accommodation_units u ON u.id = b.unit_id
            WHERE b.uid = @Uid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<UnitBlockRow>(
            new CommandDefinition(sql, new { Uid = uid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<bool> OverlapsAsync(
        long unitId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.unit_blocks
                WHERE unit_id = @UnitId
                  AND is_archived = false
                  AND is_active = true
                  AND start_date < @EndDate
                  AND end_date > @StartDate
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { UnitId = unitId, StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(UnitBlock block, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.unit_blocks
            (
                uid, organization_id, property_id, unit_id, start_date, end_date,
                block_type, reason, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @UnitId, @StartDate, @EndDate,
                @BlockType, @Reason, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                block.Uid,
                block.OrganizationId,
                block.PropertyId,
                block.UnitId,
                block.StartDate,
                block.EndDate,
                BlockType = block.BlockType.ToDatabaseValue(),
                block.Reason,
                block.IsActive,
                block.IsArchived,
                block.CreationDate,
                block.CreatedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task ArchiveAsync(UnitBlock block, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.unit_blocks
            SET is_archived = true,
                is_active = false,
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
                block.Uid,
                block.ModifiedDate,
                block.ModifiedBy
            },
            cancellationToken: cancellationToken));
    }

    private static UnitBlock ToDomain(UnitBlockRow row) => UnitBlock.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.PropertyId,
        row.PropertyUid,
        row.UnitId,
        row.UnitUid,
        row.StartDate,
        row.EndDate,
        UnitBlockTypeMapper.FromDatabaseValue(row.BlockType),
        row.Reason,
        row.IsActive,
        row.IsArchived,
        ToDateTimeOffset(row.CreationDate),
        row.CreatedBy,
        ToDateTimeOffset(row.ModifiedDate),
        row.ModifiedBy);

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value) =>
        value is null ? null : ToDateTimeOffset(value.Value);

    private sealed class UnitBlockRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public long OrganizationId { get; init; }
        public long PropertyId { get; init; }
        public Guid PropertyUid { get; init; }
        public long UnitId { get; init; }
        public Guid UnitUid { get; init; }
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
        public string BlockType { get; init; } = string.Empty;
        public string? Reason { get; init; }
        public bool IsActive { get; init; }
        public bool IsArchived { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ModifiedBy { get; init; }
    }
}
