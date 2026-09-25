using Dapper;
using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.DTOs;
using Npgsql;

namespace HMS.Modules.Booking.Infrastructure.Persistence.Repositories;

public sealed class PropertyBookingAccessRepository(IBookingDbConnectionFactory connectionFactory)
    : IPropertyBookingAccess
{
    public async Task<PropertyBookingContext?> GetByUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.id                                    AS "Id",
                p.organization_id                       AS "OrganizationId",
                COALESCE(ps.booking_number_prefix, 'BKG') AS "BookingNumberPrefix",
                p.is_archived                           AS "IsArchived"
            FROM hotel.properties p
            LEFT JOIN hotel.property_settings ps
              ON ps.property_id = p.id
             AND ps.is_archived = false
            WHERE p.uid = @PropertyUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PropertyBookingContext>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));
    }

    public async Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.app_users u
                JOIN hotel.user_property_access upa ON upa.user_id = u.id
                JOIN hotel.properties p
                  ON p.id = upa.property_id
                 AND p.organization_id = upa.organization_id
                WHERE u.auth_subject = @ActorSubject
                  AND p.uid = @PropertyUid
                  AND u.is_active = true
                  AND u.is_archived = false
                  AND upa.is_active = true
                  AND upa.is_archived = false
                  AND p.is_archived = false
                  AND
                  (
                      @RequireManager = false
                      OR upa.role_code IN ('PROPERTY_ADMIN', 'MANAGER', 'PLATFORM_ADMIN')
                  )
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new
            {
                ActorSubject = actorSubject,
                PropertyUid = propertyUid,
                RequireManager = requireManager
            },
            cancellationToken: cancellationToken));
    }
}
