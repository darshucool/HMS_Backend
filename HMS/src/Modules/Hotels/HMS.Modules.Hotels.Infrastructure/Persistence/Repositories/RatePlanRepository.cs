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
                rp.id                       AS Id,
                rp.uid                      AS Uid,
                rp.organization_id          AS OrganizationId,
                rp.property_id              AS PropertyId,
                p.uid                       AS PropertyUid,
                rp.accommodation_type_id    AS AccommodationTypeId,
                at.uid                      AS AccommodationTypeUid,
                rp.meal_plan_id             AS MealPlanId,
                mp.uid                      AS MealPlanUid,
                rp.code                     AS Code,
                rp.name                     AS Name,
                rp.pricing_basis            AS PricingBasis,
                rp.currency                 AS Currency,
                rp.description              AS Description,
                rp.is_refundable            AS IsRefundable,
                rp.is_active                AS IsActive,
                rp.is_archived              AS IsArchived,
                rp.creation_date            AS CreationDate,
                rp.created_by               AS CreatedBy,
                rp.modified_date            AS ModifiedDate,
                rp.modified_by              AS ModifiedBy
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
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.rate_plans
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

    public async Task<IReadOnlyList<RatePlanDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                rp.uid                  AS Uid,
                p.uid                   AS PropertyUid,
                at.uid                  AS AccommodationTypeUid,
                mp.uid                  AS MealPlanUid,
                rp.code                 AS Code,
                rp.name                 AS Name,
                rp.pricing_basis        AS PricingBasis,
                rp.currency             AS Currency,
                rp.description          AS Description,
                rp.is_refundable        AS IsRefundable,
                rp.is_active            AS IsActive,
                rp.creation_date        AS CreationDate
            FROM hotel.rate_plans rp
            JOIN hotel.properties p ON p.id = rp.property_id
            JOIN hotel.accommodation_types at ON at.id = rp.accommodation_type_id
            LEFT JOIN hotel.meal_plans mp ON mp.id = rp.meal_plan_id
            WHERE p.uid = @PropertyUid
              AND rp.is_archived = false
            ORDER BY rp.name, rp.code;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<RatePlanDto>(new CommandDefinition(
            sql,
            new { PropertyUid = propertyUid },
            cancellationToken: cancellationToken));

        return items.AsList();
    }

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
        row.CreationDate,
        row.CreatedBy,
        row.ModifiedDate,
        row.ModifiedBy);

    private sealed record RatePlanRow(
        long Id,
        Guid Uid,
        long OrganizationId,
        long PropertyId,
        Guid PropertyUid,
        long AccommodationTypeId,
        Guid AccommodationTypeUid,
        long? MealPlanId,
        Guid? MealPlanUid,
        string Code,
        string Name,
        string PricingBasis,
        string Currency,
        string? Description,
        bool IsRefundable,
        bool IsActive,
        bool IsArchived,
        DateTimeOffset CreationDate,
        string? CreatedBy,
        DateTimeOffset? ModifiedDate,
        string? ModifiedBy);
}
