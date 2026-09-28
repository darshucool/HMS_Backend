using Dapper;
using HMS.Modules.Payments.Application.Abstractions;

namespace HMS.Modules.Payments.Infrastructure.Persistence.Repositories;

public sealed class PaymentBookingLookup(IPaymentsDbConnectionFactory connectionFactory)
    : IPaymentBookingLookup
{
    public async Task<PaymentBookingContext?> GetByUidAsync(Guid bookingUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                b.id                AS "Id",
                b.organization_id   AS "OrganizationId",
                b.property_id       AS "PropertyId",
                p.uid               AS "PropertyUid",
                b.status            AS "Status",
                b.is_archived       AS "IsArchived"
            FROM hotel.bookings b
            JOIN hotel.properties p ON p.id = b.property_id
            WHERE b.uid = @BookingUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PaymentBookingContext>(
            new CommandDefinition(sql, new { BookingUid = bookingUid }, cancellationToken: cancellationToken));
    }

    public async Task<PaymentBookingUnitContext?> GetBookingUnitAsync(
        long bookingId,
        Guid bookingUnitUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id"
            FROM hotel.booking_units
            WHERE uid = @BookingUnitUid
              AND booking_id = @BookingId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PaymentBookingUnitContext>(
            new CommandDefinition(
                sql,
                new { BookingUnitUid = bookingUnitUid, BookingId = bookingId },
                cancellationToken: cancellationToken));
    }
}

public sealed class PaymentPropertyAccessRepository(IPaymentsDbConnectionFactory connectionFactory)
    : IPaymentPropertyAccess
{
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
