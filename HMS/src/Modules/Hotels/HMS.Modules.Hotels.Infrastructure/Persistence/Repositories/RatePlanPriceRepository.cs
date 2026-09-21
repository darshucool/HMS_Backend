using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class RatePlanPriceRepository(IHotelsDbConnectionFactory connectionFactory)
    : IRatePlanPriceRepository
{
    public async Task InsertAsync(RatePlanPrice price, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.rate_plan_prices
            (
                uid, organization_id, property_id, rate_plan_id, start_date, end_date,
                day_of_week, adult_rate, child_rate, unit_rate, minimum_stay,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @RatePlanId, @StartDate, @EndDate,
                @DayOfWeek, @AdultRate, @ChildRate, @UnitRate, @MinimumStay,
                @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                price.Uid,
                price.OrganizationId,
                price.PropertyId,
                price.RatePlanId,
                price.StartDate,
                price.EndDate,
                price.DayOfWeek,
                price.AdultRate,
                price.ChildRate,
                price.UnitRate,
                price.MinimumStay,
                price.IsActive,
                price.IsArchived,
                price.CreationDate,
                price.CreatedBy
            },
            cancellationToken: cancellationToken));
    }
}
