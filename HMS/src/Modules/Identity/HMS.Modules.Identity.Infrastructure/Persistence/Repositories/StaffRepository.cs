using Dapper;
using HMS.Modules.Identity.Application.Abstractions;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Identity.Infrastructure.Persistence.Repositories
{
    internal sealed class StaffRepository : IStaffRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public StaffRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<StaffLoginRecord?> GetForLoginAsync(
            string normalizedUsername,
            Guid propertyUid,
            CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT
                su.uid                    AS StaffUid,
                sp.property_uid           AS PropertyUid,
                su.username               AS Username,
                su.email                  AS Email,
                su.first_name             AS FirstName,
                su.last_name              AS LastName,
                su.password_hash          AS PasswordHash,
                su.is_active              AS IsActive,
                su.is_locked              AS IsLocked,
                su.failed_login_count     AS FailedLoginCount,
                su.locked_until_utc       AS LockedUntilUtc,
                COALESCE(
                    ARRAY_AGG(sr.code)
                        FILTER (WHERE sr.code IS NOT NULL),
                    ARRAY[]::VARCHAR[]
                )                         AS Roles
            FROM identity.staff_user su
            INNER JOIN identity.staff_property sp
                ON sp.staff_uid = su.uid
               AND sp.property_uid = @PropertyUid
               AND sp.is_active = TRUE
            LEFT JOIN identity.staff_user_role sur
                ON sur.staff_property_uid = sp.uid
            LEFT JOIN identity.staff_role sr
                ON sr.uid = sur.role_uid
               AND sr.is_active = TRUE
            WHERE su.normalized_username = @NormalizedUsername
            GROUP BY
                su.uid,
                sp.property_uid,
                su.username,
                su.email,
                su.first_name,
                su.last_name,
                su.password_hash,
                su.is_active,
                su.is_locked,
                su.failed_login_count,
                su.locked_until_utc
            LIMIT 1;
            """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<StaffLoginRecord>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        NormalizedUsername = normalizedUsername,
                        PropertyUid = propertyUid
                    },
                    cancellationToken: cancellationToken));
        }

        public async Task RecordSuccessfulLoginAsync(
            Guid staffUid,
            CancellationToken cancellationToken)
        {
            const string sql = """
            UPDATE identity.staff_user
            SET
                failed_login_count = 0,
                locked_until_utc = NULL,
                last_login_utc = NOW(),
                updated_at_utc = NOW()
            WHERE uid = @StaffUid;
            """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new { StaffUid = staffUid },
                    cancellationToken: cancellationToken));
        }

        public async Task RecordFailedLoginAsync(
            Guid staffUid,
            int maximumFailedAttempts,
            TimeSpan lockDuration,
            CancellationToken cancellationToken)
        {
            const string sql = """
            UPDATE identity.staff_user
            SET
                failed_login_count = failed_login_count + 1,
                locked_until_utc =
                    CASE
                        WHEN failed_login_count + 1 >= @MaximumFailedAttempts
                        THEN NOW() + (@LockDurationSeconds * INTERVAL '1 second')
                        ELSE locked_until_utc
                    END,
                updated_at_utc = NOW()
            WHERE uid = @StaffUid;
            """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        StaffUid = staffUid,
                        MaximumFailedAttempts = maximumFailedAttempts,
                        LockDurationSeconds = (int)lockDuration.TotalSeconds
                    },
                    cancellationToken: cancellationToken));
        }
    }
}
