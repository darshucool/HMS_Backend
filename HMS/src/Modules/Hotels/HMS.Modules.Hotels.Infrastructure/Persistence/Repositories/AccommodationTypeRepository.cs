using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class AccommodationTypeRepository(IHotelsDbConnectionFactory connectionFactory)
    : IAccommodationTypeRepository
{
    public async Task<AccommodationType?> GetByUidAsync(Guid uid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                at.id                   AS Id,
                at.uid                  AS Uid,
                at.organization_id      AS OrganizationId,
                at.property_id          AS PropertyId,
                p.uid                   AS PropertyUid,
                at.code                 AS Code,
                at.name                 AS Name,
                at.unit_kind            AS UnitKind,
                at.description          AS Description,
                at.max_adults           AS MaxAdults,
                at.max_children         AS MaxChildren,
                at.max_occupancy        AS MaxOccupancy,
                at.default_quantity     AS DefaultQuantity,
                at.base_rate            AS BaseRate,
                at.sort_order           AS SortOrder,
                at.is_active            AS IsActive,
                at.is_archived          AS IsArchived,
                at.creation_date        AS CreationDate,
                at.created_by           AS CreatedBy,
                at.modified_date        AS ModifiedDate,
                at.modified_by          AS ModifiedBy
            FROM hotel.accommodation_types at
            JOIN hotel.properties p ON p.id = at.property_id
            WHERE at.uid = @Uid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<AccommodationTypeRow>(
            new CommandDefinition(sql, new { Uid = uid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<bool> CodeExistsAsync(
        long propertyId,
        string code,
        Guid? excludeUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.accommodation_types
                WHERE property_id = @PropertyId
                  AND code = @Code
                  AND (@ExcludeUid IS NULL OR uid <> @ExcludeUid)
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new
            {
                PropertyId = propertyId,
                Code = code.Trim().ToUpperInvariant(),
                ExcludeUid = excludeUid
            },
            cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(AccommodationType accommodationType, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.accommodation_types
            (
                uid, organization_id, property_id, code, name, unit_kind, description,
                max_adults, max_children, max_occupancy, default_quantity, base_rate,
                sort_order, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @Code, @Name, @UnitKind, @Description,
                @MaxAdults, @MaxChildren, @MaxOccupancy, @DefaultQuantity, @BaseRate,
                @SortOrder, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        var parameters = new
        {
            accommodationType.Uid,
            accommodationType.OrganizationId,
            accommodationType.PropertyId,
            accommodationType.Code,
            accommodationType.Name,
            UnitKind = accommodationType.UnitKind.ToDatabaseValue(),
            accommodationType.Description,
            accommodationType.MaxAdults,
            accommodationType.MaxChildren,
            accommodationType.MaxOccupancy,
            accommodationType.DefaultQuantity,
            accommodationType.BaseRate,
            accommodationType.SortOrder,
            accommodationType.IsActive,
            accommodationType.IsArchived,
            accommodationType.CreationDate,
            accommodationType.CreatedBy
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(AccommodationType accommodationType, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.accommodation_types
            SET code = @Code,
                name = @Name,
                unit_kind = @UnitKind,
                description = @Description,
                max_adults = @MaxAdults,
                max_children = @MaxChildren,
                max_occupancy = @MaxOccupancy,
                default_quantity = @DefaultQuantity,
                base_rate = @BaseRate,
                sort_order = @SortOrder,
                is_active = @IsActive,
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
                accommodationType.Uid,
                accommodationType.Code,
                accommodationType.Name,
                UnitKind = accommodationType.UnitKind.ToDatabaseValue(),
                accommodationType.Description,
                accommodationType.MaxAdults,
                accommodationType.MaxChildren,
                accommodationType.MaxOccupancy,
                accommodationType.DefaultQuantity,
                accommodationType.BaseRate,
                accommodationType.SortOrder,
                accommodationType.IsActive,
                accommodationType.ModifiedDate,
                accommodationType.ModifiedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task ArchiveAsync(AccommodationType accommodationType, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.accommodation_types
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
                accommodationType.Uid,
                accommodationType.ModifiedDate,
                accommodationType.ModifiedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AccommodationTypeDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                at.uid                  AS Uid,
                p.uid                   AS PropertyUid,
                at.code                 AS Code,
                at.name                 AS Name,
                at.unit_kind            AS UnitKind,
                at.description          AS Description,
                at.max_adults           AS MaxAdults,
                at.max_children         AS MaxChildren,
                at.max_occupancy        AS MaxOccupancy,
                at.default_quantity     AS DefaultQuantity,
                at.base_rate            AS BaseRate,
                at.sort_order           AS SortOrder,
                at.is_active            AS IsActive,
                at.creation_date        AS CreationDate
            FROM hotel.accommodation_types at
            JOIN hotel.properties p ON p.id = at.property_id
            WHERE p.uid = @PropertyUid
              AND at.is_archived = false
            ORDER BY at.sort_order, at.name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<AccommodationTypeDto>(new CommandDefinition(
            sql,
            new { PropertyUid = propertyUid },
            cancellationToken: cancellationToken));

        return items.AsList();
    }

    private static AccommodationType ToDomain(AccommodationTypeRow row) => AccommodationType.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.PropertyId,
        row.PropertyUid,
        row.Code,
        row.Name,
        UnitKindMapper.FromDatabaseValue(row.UnitKind),
        row.Description,
        row.MaxAdults,
        row.MaxChildren,
        row.MaxOccupancy,
        row.DefaultQuantity,
        row.BaseRate,
        row.SortOrder,
        row.IsActive,
        row.IsArchived,
        row.CreationDate,
        row.CreatedBy,
        row.ModifiedDate,
        row.ModifiedBy);

    private sealed record AccommodationTypeRow(
        long Id,
        Guid Uid,
        long OrganizationId,
        long PropertyId,
        Guid PropertyUid,
        string Code,
        string Name,
        string UnitKind,
        string? Description,
        int MaxAdults,
        int MaxChildren,
        int MaxOccupancy,
        int DefaultQuantity,
        decimal BaseRate,
        int SortOrder,
        bool IsActive,
        bool IsArchived,
        DateTimeOffset CreationDate,
        string? CreatedBy,
        DateTimeOffset? ModifiedDate,
        string? ModifiedBy);
}
