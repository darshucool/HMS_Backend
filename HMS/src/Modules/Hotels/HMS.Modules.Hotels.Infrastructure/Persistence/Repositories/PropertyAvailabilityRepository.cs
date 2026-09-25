using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.DTOs;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class PropertyAvailabilityRepository(IHotelsDbConnectionFactory connectionFactory)
    : IPropertyAvailabilityRepository
{
    public async Task<IReadOnlyList<PropertyAvailabilityItemDto>> GetAsync(
        Guid propertyUid,
        DateOnly checkIn,
        DateOnly checkOut,
        int adults,
        int children,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH eligible_types AS
            (
                SELECT at.*
                FROM hotel.accommodation_types at
                JOIN hotel.properties p ON p.id = at.property_id
                WHERE p.uid = @PropertyUid
                  AND at.is_archived = false
                  AND at.is_active = true
                  AND at.max_adults >= @Adults
                  AND at.max_children >= @Children
                  AND at.max_occupancy >= (@Adults + @Children)
            ),
            physical_units AS
            (
                SELECT u.accommodation_type_id, u.id AS unit_id
                FROM hotel.accommodation_units u
                JOIN eligible_types t ON t.id = u.accommodation_type_id
                WHERE u.is_archived = false
                  AND u.is_active = true
                  AND u.status NOT IN ('OUT_OF_SERVICE', 'MAINTENANCE', 'INACTIVE')
            ),
            blocked_units AS
            (
                SELECT DISTINCT b.unit_id
                FROM hotel.unit_blocks b
                JOIN physical_units pu ON pu.unit_id = b.unit_id
                WHERE b.is_archived = false
                  AND b.is_active = true
                  AND b.start_date < @CheckOut
                  AND b.end_date > @CheckIn
            ),
            booked_units AS
            (
                SELECT DISTINCT bu.unit_id
                FROM hotel.booking_units bu
                JOIN physical_units pu ON pu.unit_id = bu.unit_id
                WHERE bu.is_archived = false
                  AND bu.unit_id IS NOT NULL
                  AND bu.allocation_status IN ('HELD', 'CONFIRMED', 'CHECKED_IN')
                  AND bu.check_in_date < @CheckOut
                  AND bu.check_out_date > @CheckIn
            ),
            available_units AS
            (
                SELECT
                    pu.accommodation_type_id,
                    COUNT(*)::int AS available_count
                FROM physical_units pu
                WHERE NOT EXISTS (SELECT 1 FROM blocked_units b WHERE b.unit_id = pu.unit_id)
                  AND NOT EXISTS (SELECT 1 FROM booked_units b WHERE b.unit_id = pu.unit_id)
                GROUP BY pu.accommodation_type_id
            ),
            unassigned_holds AS
            (
                SELECT
                    bu.accommodation_type_id,
                    COALESCE(SUM(bu.unit_quantity), 0)::int AS held_quantity
                FROM hotel.booking_units bu
                JOIN eligible_types t ON t.id = bu.accommodation_type_id
                WHERE bu.is_archived = false
                  AND bu.unit_id IS NULL
                  AND bu.allocation_status IN ('HELD', 'CONFIRMED', 'CHECKED_IN')
                  AND bu.check_in_date < @CheckOut
                  AND bu.check_out_date > @CheckIn
                GROUP BY bu.accommodation_type_id
            ),
            total_units AS
            (
                SELECT
                    u.accommodation_type_id,
                    COUNT(*)::int AS total_count
                FROM hotel.accommodation_units u
                JOIN eligible_types t ON t.id = u.accommodation_type_id
                WHERE u.is_archived = false
                  AND u.is_active = true
                GROUP BY u.accommodation_type_id
            )
            SELECT
                t.uid AS AccommodationTypeUid,
                t.code AS Code,
                t.name AS Name,
                t.unit_kind AS UnitKind,
                t.max_adults AS MaxAdults,
                t.max_children AS MaxChildren,
                t.max_occupancy AS MaxOccupancy,
                CASE
                    WHEN COALESCE(tu.total_count, 0) = 0
                        THEN GREATEST(t.default_quantity - COALESCE(uh.held_quantity, 0), 0)
                    ELSE GREATEST(COALESCE(au.available_count, 0) - COALESCE(uh.held_quantity, 0), 0)
                END AS AvailableUnits,
                COALESCE(tu.total_count, t.default_quantity) AS TotalUnits,
                t.base_rate AS BaseRate,
                (t.base_rate * (@CheckOut::date - @CheckIn::date)) AS EstimatedTotal
            FROM eligible_types t
            LEFT JOIN available_units au ON au.accommodation_type_id = t.id
            LEFT JOIN unassigned_holds uh ON uh.accommodation_type_id = t.id
            LEFT JOIN total_units tu ON tu.accommodation_type_id = t.id
            ORDER BY t.sort_order, t.name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<PropertyAvailabilityItemDto>(new CommandDefinition(
            sql,
            new
            {
                PropertyUid = propertyUid,
                CheckIn = checkIn,
                CheckOut = checkOut,
                Adults = adults,
                Children = children
            },
            cancellationToken: cancellationToken));

        return items.AsList();
    }
}
