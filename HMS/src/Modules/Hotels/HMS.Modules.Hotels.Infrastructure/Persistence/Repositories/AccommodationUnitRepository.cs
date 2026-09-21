using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class AccommodationUnitRepository(IHotelsDbConnectionFactory connectionFactory)
    : IAccommodationUnitRepository
{
    public async Task<AccommodationUnit?> GetByUidAsync(Guid uid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u.id                    AS Id,
                u.uid                   AS Uid,
                u.organization_id       AS OrganizationId,
                u.property_id           AS PropertyId,
                p.uid                   AS PropertyUid,
                u.accommodation_type_id AS AccommodationTypeId,
                at.uid                  AS AccommodationTypeUid,
                u.unit_code             AS UnitCode,
                u.unit_name             AS UnitName,
                u.floor_or_area         AS FloorOrArea,
                u.status                AS Status,
                u.housekeeping_status   AS HousekeepingStatus,
                u.notes                 AS Notes,
                u.is_active             AS IsActive,
                u.is_archived           AS IsArchived,
                u.creation_date         AS CreationDate,
                u.created_by            AS CreatedBy,
                u.modified_date         AS ModifiedDate,
                u.modified_by           AS ModifiedBy
            FROM hotel.accommodation_units u
            JOIN hotel.properties p ON p.id = u.property_id
            JOIN hotel.accommodation_types at ON at.id = u.accommodation_type_id
            WHERE u.uid = @Uid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<AccommodationUnitRow>(
            new CommandDefinition(sql, new { Uid = uid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<bool> UnitCodeExistsAsync(
        long propertyId,
        string unitCode,
        Guid? excludeUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.accommodation_units
                WHERE property_id = @PropertyId
                  AND unit_code = @UnitCode
                  AND (@ExcludeUid IS NULL OR uid <> @ExcludeUid)
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new
            {
                PropertyId = propertyId,
                UnitCode = unitCode.Trim(),
                ExcludeUid = excludeUid
            },
            cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(AccommodationUnit unit, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.accommodation_units
            (
                uid, organization_id, property_id, accommodation_type_id, unit_code,
                unit_name, floor_or_area, status, housekeeping_status, notes,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @AccommodationTypeId, @UnitCode,
                @UnitName, @FloorOrArea, @Status, @HousekeepingStatus, @Notes,
                @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            ToWriteParameters(unit),
            cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(AccommodationUnit unit, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.accommodation_units
            SET accommodation_type_id = @AccommodationTypeId,
                unit_code = @UnitCode,
                unit_name = @UnitName,
                floor_or_area = @FloorOrArea,
                status = @Status,
                housekeeping_status = @HousekeepingStatus,
                notes = @Notes,
                is_active = @IsActive,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE uid = @Uid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            ToWriteParameters(unit),
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AccommodationUnitDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u.uid                   AS Uid,
                p.uid                   AS PropertyUid,
                at.uid                  AS AccommodationTypeUid,
                u.unit_code             AS UnitCode,
                u.unit_name             AS UnitName,
                u.floor_or_area         AS FloorOrArea,
                u.status                AS Status,
                u.housekeeping_status   AS HousekeepingStatus,
                u.notes                 AS Notes,
                u.is_active             AS IsActive,
                u.creation_date         AS CreationDate
            FROM hotel.accommodation_units u
            JOIN hotel.properties p ON p.id = u.property_id
            JOIN hotel.accommodation_types at ON at.id = u.accommodation_type_id
            WHERE p.uid = @PropertyUid
              AND u.is_archived = false
            ORDER BY u.unit_code;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<AccommodationUnitDto>(new CommandDefinition(
            sql,
            new { PropertyUid = propertyUid },
            cancellationToken: cancellationToken));

        return items.AsList();
    }

    private static object ToWriteParameters(AccommodationUnit unit) => new
    {
        unit.Uid,
        unit.OrganizationId,
        unit.PropertyId,
        unit.AccommodationTypeId,
        unit.UnitCode,
        unit.UnitName,
        unit.FloorOrArea,
        Status = unit.Status.ToDatabaseValue(),
        HousekeepingStatus = unit.HousekeepingStatus.ToDatabaseValue(),
        unit.Notes,
        unit.IsActive,
        unit.IsArchived,
        unit.CreationDate,
        unit.CreatedBy,
        unit.ModifiedDate,
        unit.ModifiedBy
    };

    private static AccommodationUnit ToDomain(AccommodationUnitRow row) => AccommodationUnit.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.PropertyId,
        row.PropertyUid,
        row.AccommodationTypeId,
        row.AccommodationTypeUid,
        row.UnitCode,
        row.UnitName,
        row.FloorOrArea,
        AccommodationUnitStatusMapper.FromDatabaseValue(row.Status),
        HousekeepingStatusMapper.FromDatabaseValue(row.HousekeepingStatus),
        row.Notes,
        row.IsActive,
        row.IsArchived,
        row.CreationDate,
        row.CreatedBy,
        row.ModifiedDate,
        row.ModifiedBy);

    private sealed record AccommodationUnitRow(
        long Id,
        Guid Uid,
        long OrganizationId,
        long PropertyId,
        Guid PropertyUid,
        long AccommodationTypeId,
        Guid AccommodationTypeUid,
        string UnitCode,
        string? UnitName,
        string? FloorOrArea,
        string Status,
        string HousekeepingStatus,
        string? Notes,
        bool IsActive,
        bool IsArchived,
        DateTimeOffset CreationDate,
        string? CreatedBy,
        DateTimeOffset? ModifiedDate,
        string? ModifiedBy);
}
