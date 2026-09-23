using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class MealPlanRepository(IHotelsDbConnectionFactory connectionFactory)
    : IMealPlanRepository
{
    public async Task<MealPlan?> GetByUidAsync(Guid uid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                mp.id                   AS "Id",
                mp.uid                  AS "Uid",
                mp.organization_id      AS "OrganizationId",
                mp.property_id          AS "PropertyId",
                p.uid                   AS "PropertyUid",
                mp.code                 AS "Code",
                mp.name                 AS "Name",
                mp.description          AS "Description",
                mp.includes_breakfast   AS "IncludesBreakfast",
                mp.includes_lunch       AS "IncludesLunch",
                mp.includes_dinner      AS "IncludesDinner",
                mp.allow_byo            AS "AllowByo",
                mp.is_active            AS "IsActive",
                mp.is_archived          AS "IsArchived",
                mp.creation_date        AS "CreationDate",
                mp.created_by           AS "CreatedBy",
                mp.modified_date        AS "ModifiedDate",
                mp.modified_by          AS "ModifiedBy"
            FROM hotel.meal_plans mp
            JOIN hotel.properties p ON p.id = mp.property_id
            WHERE mp.uid = @Uid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<MealPlanRow>(
            new CommandDefinition(sql, new { Uid = uid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<bool> CodeExistsAsync(
        long propertyId,
        string code,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.meal_plans
                WHERE property_id = @PropertyId
                  AND code = @Code
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { PropertyId = propertyId, Code = code.Trim().ToUpperInvariant() },
            cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(MealPlan mealPlan, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.meal_plans
            (
                uid, organization_id, property_id, code, name, description,
                includes_breakfast, includes_lunch, includes_dinner, allow_byo,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @Code, @Name, @Description,
                @IncludesBreakfast, @IncludesLunch, @IncludesDinner, @AllowByo,
                @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                mealPlan.Uid,
                mealPlan.OrganizationId,
                mealPlan.PropertyId,
                mealPlan.Code,
                mealPlan.Name,
                mealPlan.Description,
                mealPlan.IncludesBreakfast,
                mealPlan.IncludesLunch,
                mealPlan.IncludesDinner,
                mealPlan.AllowByo,
                mealPlan.IsActive,
                mealPlan.IsArchived,
                mealPlan.CreationDate,
                mealPlan.CreatedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<MealPlanDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                mp.uid                  AS "Uid",
                p.uid                   AS "PropertyUid",
                mp.code                 AS "Code",
                mp.name                 AS "Name",
                mp.description          AS "Description",
                mp.includes_breakfast   AS "IncludesBreakfast",
                mp.includes_lunch       AS "IncludesLunch",
                mp.includes_dinner      AS "IncludesDinner",
                mp.allow_byo            AS "AllowByo",
                mp.is_active            AS "IsActive",
                mp.creation_date        AS "CreationDate"
            FROM hotel.meal_plans mp
            JOIN hotel.properties p ON p.id = mp.property_id
            WHERE p.uid = @PropertyUid
              AND mp.is_archived = false
            ORDER BY mp.name, mp.code;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<MealPlanListRow>(new CommandDefinition(
            sql,
            new { PropertyUid = propertyUid },
            cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    private static MealPlan ToDomain(MealPlanRow row) => MealPlan.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.PropertyId,
        row.PropertyUid,
        row.Code,
        row.Name,
        row.Description,
        row.IncludesBreakfast,
        row.IncludesLunch,
        row.IncludesDinner,
        row.AllowByo,
        row.IsActive,
        row.IsArchived,
        ToDateTimeOffset(row.CreationDate),
        row.CreatedBy,
        ToDateTimeOffset(row.ModifiedDate),
        row.ModifiedBy);

    private static MealPlanDto ToDto(MealPlanListRow row) => new(
        row.Uid,
        row.PropertyUid,
        row.Code,
        row.Name,
        row.Description,
        row.IncludesBreakfast,
        row.IncludesLunch,
        row.IncludesDinner,
        row.AllowByo,
        row.IsActive,
        ToDateTimeOffset(row.CreationDate));

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value) =>
        value is null ? null : ToDateTimeOffset(value.Value);

    private sealed class MealPlanRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public long OrganizationId { get; init; }
        public long PropertyId { get; init; }
        public Guid PropertyUid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IncludesBreakfast { get; init; }
        public bool IncludesLunch { get; init; }
        public bool IncludesDinner { get; init; }
        public bool AllowByo { get; init; }
        public bool IsActive { get; init; }
        public bool IsArchived { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ModifiedBy { get; init; }
    }

    private sealed class MealPlanListRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IncludesBreakfast { get; init; }
        public bool IncludesLunch { get; init; }
        public bool IncludesDinner { get; init; }
        public bool AllowByo { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
