using Dapper;
using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.DTOs;
using HMS.Modules.Payments.Domain.Enums;

namespace HMS.Modules.Payments.Infrastructure.Persistence.Repositories;

public sealed class BookingChargeTypeRepository(IPaymentsDbConnectionFactory connectionFactory)
    : IBookingChargeTypeRepository
{
    public async Task<IReadOnlyList<BookingChargeTypeDto>> ListAsync(
        long propertyId,
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid             AS "Uid",
                code            AS "Code",
                name            AS "Name",
                category        AS "Category",
                is_taxable      AS "IsTaxable",
                default_price   AS "DefaultPrice",
                is_active       AS "IsActive"
            FROM hotel.booking_charge_types
            WHERE property_id = @PropertyId
              AND is_archived = false
            ORDER BY name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ChargeTypeRow>(
            new CommandDefinition(sql, new { PropertyId = propertyId }, cancellationToken: cancellationToken));

        return rows.Select(row => ToDto(row, propertyUid)).ToList();
    }

    public async Task<BookingChargeTypeContext?> GetByUidAsync(
        Guid chargeTypeUid,
        long propertyId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                t.id        AS "Id",
                t.uid       AS "Uid",
                p.uid       AS "PropertyUid",
                t.is_archived AS "IsArchived"
            FROM hotel.booking_charge_types t
            JOIN hotel.properties p ON p.id = t.property_id
            WHERE t.uid = @ChargeTypeUid
              AND t.property_id = @PropertyId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ChargeTypeContextRow>(
            new CommandDefinition(
                sql,
                new { ChargeTypeUid = chargeTypeUid, PropertyId = propertyId },
                cancellationToken: cancellationToken));

        return row is null
            ? null
            : new BookingChargeTypeContext(row.Id, row.Uid, row.PropertyUid, row.IsArchived);
    }

    public async Task<BookingChargeTypeDto?> GetDetailAsync(
        Guid chargeTypeUid,
        long propertyId,
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid             AS "Uid",
                code            AS "Code",
                name            AS "Name",
                category        AS "Category",
                is_taxable      AS "IsTaxable",
                default_price   AS "DefaultPrice",
                is_active       AS "IsActive"
            FROM hotel.booking_charge_types
            WHERE uid = @ChargeTypeUid
              AND property_id = @PropertyId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ChargeTypeRow>(
            new CommandDefinition(
                sql,
                new { ChargeTypeUid = chargeTypeUid, PropertyId = propertyId },
                cancellationToken: cancellationToken));

        return row is null ? null : ToDto(row, propertyUid);
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
                FROM hotel.booking_charge_types
                WHERE property_id = @PropertyId
                  AND code = @Code
                  AND (@ExcludeUid IS NULL OR uid <> @ExcludeUid)
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { PropertyId = propertyId, Code = code, ExcludeUid = excludeUid },
            cancellationToken: cancellationToken));
    }

    public async Task<Guid> InsertAsync(
        long organizationId,
        long propertyId,
        string code,
        string name,
        string category,
        bool isTaxable,
        decimal? defaultPrice,
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.booking_charge_types
            (
                uid, organization_id, property_id, code, name, category,
                is_taxable, default_price, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @Code, @Name, @Category,
                @IsTaxable, @DefaultPrice, @IsActive, false, @CreationDate, @CreatedBy
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
                Code = code,
                Name = name,
                Category = category,
                IsTaxable = isTaxable,
                DefaultPrice = defaultPrice,
                IsActive = isActive,
                CreationDate = DateTime.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task UpdateAsync(
        long chargeTypeId,
        string code,
        string name,
        string category,
        bool isTaxable,
        decimal? defaultPrice,
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.booking_charge_types
            SET code = @Code,
                name = @Name,
                category = @Category,
                is_taxable = @IsTaxable,
                default_price = @DefaultPrice,
                is_active = @IsActive,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @ChargeTypeId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                ChargeTypeId = chargeTypeId,
                Code = code,
                Name = name,
                Category = category,
                IsTaxable = isTaxable,
                DefaultPrice = defaultPrice,
                IsActive = isActive,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = actorSubject
            },
            cancellationToken: cancellationToken));
    }

    public async Task ArchiveAsync(long chargeTypeId, string actorSubject, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.booking_charge_types
            SET is_archived = true,
                is_active = false,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @ChargeTypeId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                ChargeTypeId = chargeTypeId,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = actorSubject
            },
            cancellationToken: cancellationToken));
    }

    private static BookingChargeTypeDto ToDto(ChargeTypeRow row, Guid propertyUid) => new(
        row.Uid,
        propertyUid,
        row.Code,
        row.Name,
        BookingChargeCategoryMapper.FromDatabaseValue(row.Category),
        row.IsTaxable,
        row.DefaultPrice,
        row.IsActive);

    private sealed class ChargeTypeRow
    {
        public Guid Uid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public bool IsTaxable { get; init; }
        public decimal? DefaultPrice { get; init; }
        public bool IsActive { get; init; }
    }

    private sealed class ChargeTypeContextRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public bool IsArchived { get; init; }
    }
}
