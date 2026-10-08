using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class RatePlanRepository(IHotelsDbConnectionFactory connectionFactory)
    : IRatePlanRepository
{
    public async Task<RatePlan?> GetByUidAsync(Guid uid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                rp.id                       AS "Id",
                rp.uid                      AS "Uid",
                rp.organization_id          AS "OrganizationId",
                rp.property_id              AS "PropertyId",
                p.uid                       AS "PropertyUid",
                rp.accommodation_type_id    AS "AccommodationTypeId",
                at.uid                      AS "AccommodationTypeUid",
                rp.meal_plan_id             AS "MealPlanId",
                mp.uid                      AS "MealPlanUid",
                rp.code                     AS "Code",
                rp.name                     AS "Name",
                rp.pricing_basis            AS "PricingBasis",
                rtrim(rp.currency)          AS "Currency",
                rp.description              AS "Description",
                rp.is_refundable            AS "IsRefundable",
                rp.is_active                AS "IsActive",
                rp.is_archived              AS "IsArchived",
                rp.creation_date            AS "CreationDate",
                rp.created_by               AS "CreatedBy",
                rp.modified_date            AS "ModifiedDate",
                rp.modified_by              AS "ModifiedBy"
            FROM hotel.rate_plans rp
            JOIN hotel.properties p ON p.id = rp.property_id
            JOIN hotel.accommodation_types at ON at.id = rp.accommodation_type_id
            LEFT JOIN hotel.meal_plans mp ON mp.id = rp.meal_plan_id
            WHERE rp.uid = @Uid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<RatePlanRow>(
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
                FROM hotel.rate_plans
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

    public async Task InsertAsync(RatePlan ratePlan, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.rate_plans
            (
                uid, organization_id, property_id, accommodation_type_id, meal_plan_id,
                code, name, pricing_basis, currency, description, is_refundable,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @AccommodationTypeId, @MealPlanId,
                @Code, @Name, @PricingBasis, @Currency, @Description, @IsRefundable,
                @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                ratePlan.Uid,
                ratePlan.OrganizationId,
                ratePlan.PropertyId,
                ratePlan.AccommodationTypeId,
                ratePlan.MealPlanId,
                ratePlan.Code,
                ratePlan.Name,
                PricingBasis = ratePlan.PricingBasis.ToDatabaseValue(),
                ratePlan.Currency,
                ratePlan.Description,
                ratePlan.IsRefundable,
                ratePlan.IsActive,
                ratePlan.IsArchived,
                ratePlan.CreationDate,
                ratePlan.CreatedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(RatePlan ratePlan, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.rate_plans
            SET code = @Code,
                name = @Name,
                pricing_basis = @PricingBasis,
                currency = @Currency,
                description = @Description,
                is_refundable = @IsRefundable,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @Id
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                ratePlan.Id,
                ratePlan.Code,
                ratePlan.Name,
                PricingBasis = ratePlan.PricingBasis.ToDatabaseValue(),
                ratePlan.Currency,
                ratePlan.Description,
                ratePlan.IsRefundable,
                ratePlan.ModifiedDate,
                ratePlan.ModifiedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task ArchiveAsync(RatePlan ratePlan, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.rate_plans
            SET is_active = false,
                is_archived = true,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @Id
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                ratePlan.Id,
                ratePlan.ModifiedDate,
                ratePlan.ModifiedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<RatePlanDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                rp.uid                  AS "Uid",
                p.uid                   AS "PropertyUid",
                at.uid                  AS "AccommodationTypeUid",
                mp.uid                  AS "MealPlanUid",
                rp.code                 AS "Code",
                rp.name                 AS "Name",
                rp.pricing_basis        AS "PricingBasis",
                rtrim(rp.currency)      AS "Currency",
                rp.description          AS "Description",
                rp.is_refundable        AS "IsRefundable",
                rp.is_active            AS "IsActive",
                rp.creation_date        AS "CreationDate"
            FROM hotel.rate_plans rp
            JOIN hotel.properties p ON p.id = rp.property_id
            JOIN hotel.accommodation_types at ON at.id = rp.accommodation_type_id
            LEFT JOIN hotel.meal_plans mp ON mp.id = rp.meal_plan_id
            WHERE p.uid = @PropertyUid
              AND rp.is_archived = false
            ORDER BY rp.name, rp.code;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<RatePlanListRow>(new CommandDefinition(
            sql,
            new { PropertyUid = propertyUid },
            cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    private static RatePlanDto ToDto(RatePlanListRow row) => new(
        row.Uid,
        row.PropertyUid,
        row.AccommodationTypeUid,
        row.MealPlanUid,
        row.Code,
        row.Name,
        row.PricingBasis,
        row.Currency,
        row.Description,
        row.IsRefundable,
        row.IsActive,
        ToDateTimeOffset(row.CreationDate));

    private static RatePlan ToDomain(RatePlanRow row) => RatePlan.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.PropertyId,
        row.PropertyUid,
        row.AccommodationTypeId,
        row.AccommodationTypeUid,
        row.MealPlanId,
        row.MealPlanUid,
        row.Code,
        row.Name,
        PricingBasisMapper.FromDatabaseValue(row.PricingBasis),
        row.Currency,
        row.Description,
        row.IsRefundable,
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

    private sealed class RatePlanRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public long OrganizationId { get; init; }
        public long PropertyId { get; init; }
        public Guid PropertyUid { get; init; }
        public long AccommodationTypeId { get; init; }
        public Guid AccommodationTypeUid { get; init; }
        public long? MealPlanId { get; init; }
        public Guid? MealPlanUid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string PricingBasis { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsRefundable { get; init; }
        public bool IsActive { get; init; }
        public bool IsArchived { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ModifiedBy { get; init; }
    }

    private sealed class RatePlanListRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public Guid AccommodationTypeUid { get; init; }
        public Guid? MealPlanUid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string PricingBasis { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsRefundable { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
