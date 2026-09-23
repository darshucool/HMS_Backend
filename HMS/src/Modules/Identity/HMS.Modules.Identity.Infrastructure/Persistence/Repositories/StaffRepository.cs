using Dapper;
using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Domain.Entities;
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

        public async Task<bool> UsernameExistsAsync(
            string normalizedUsername,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT EXISTS
                (
                    SELECT 1
                    FROM identity.staff_user
                    WHERE normalized_username = @NormalizedUsername
                );
                """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    sql,
                    new { NormalizedUsername = normalizedUsername },
                    cancellationToken: cancellationToken));
        }

        public async Task<bool> EmailExistsAsync(
            string normalizedEmail,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT EXISTS
                (
                    SELECT 1
                    FROM identity.staff_user
                    WHERE normalized_email = @NormalizedEmail
                );
                """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    sql,
                    new { NormalizedEmail = normalizedEmail },
                    cancellationToken: cancellationToken));
        }

        public async Task<bool> RoleExistsAsync(
            string roleCode,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT EXISTS
                (
                    SELECT 1
                    FROM identity.staff_role
                    WHERE code = @RoleCode
                      AND is_active = TRUE
                );
                """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    sql,
                    new { RoleCode = roleCode },
                    cancellationToken: cancellationToken));
        }

        public async Task<PropertyLookup?> GetPropertyByUidAsync(
            Guid propertyUid,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    p.id                AS Id,
                    p.organization_id   AS OrganizationId,
                    p.uid               AS Uid,
                    p.is_archived       AS IsArchived
                FROM hotel.properties p
                WHERE p.uid = @PropertyUid;
                """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<PropertyLookup>(
                new CommandDefinition(
                    sql,
                    new { PropertyUid = propertyUid },
                    cancellationToken: cancellationToken));
        }

        public async Task InsertRegisteredUserAsync(
            StaffUser staff,
            Guid propertyUid,
            string identityRoleCode,
            string hotelAccessRoleCode,
            Guid? createdBy,
            CancellationToken cancellationToken)
        {
            const string insertStaffSql = """
                INSERT INTO identity.staff_user
                (
                    uid, username, normalized_username, email, normalized_email,
                    first_name, last_name, password_hash, is_active, created_at_utc, created_by
                )
                VALUES
                (
                    @Uid, @Username, @NormalizedUsername, @Email, @NormalizedEmail,
                    @FirstName, @LastName, @PasswordHash, TRUE, NOW(), @CreatedBy
                );
                """;

            const string insertPropertySql = """
                INSERT INTO identity.staff_property
                    (staff_uid, property_uid, is_active, assigned_at_utc)
                VALUES
                    (@StaffUid, @PropertyUid, TRUE, NOW())
                RETURNING uid;
                """;

            const string insertRoleSql = """
                INSERT INTO identity.staff_user_role
                    (staff_property_uid, role_uid, assigned_at_utc)
                SELECT @StaffPropertyUid, sr.uid, NOW()
                FROM identity.staff_role sr
                WHERE sr.code = @RoleCode
                  AND sr.is_active = TRUE;
                """;

            const string upsertAppUserSql = """
                INSERT INTO hotel.app_users
                (
                    uid, auth_subject, user_name, first_name, last_name, email,
                    status, is_active, is_archived, creation_date, created_by
                )
                VALUES
                (
                    @AppUserUid, @AuthSubject, @Username, @FirstName, @LastName, @Email,
                    'ACTIVE', TRUE, FALSE, NOW(), @CreatedBySubject
                )
                ON CONFLICT (auth_subject) DO UPDATE
                SET
                    user_name = EXCLUDED.user_name,
                    first_name = EXCLUDED.first_name,
                    last_name = EXCLUDED.last_name,
                    email = EXCLUDED.email,
                    status = 'ACTIVE',
                    is_active = TRUE,
                    is_archived = FALSE,
                    modified_date = NOW(),
                    modified_by = EXCLUDED.created_by
                RETURNING id;
                """;

            const string insertAccessSql = """
                INSERT INTO hotel.user_property_access
                (
                    user_id, organization_id, property_id, role_code,
                    is_default_property, is_active, is_archived, creation_date, created_by
                )
                VALUES
                (
                    @UserId, @OrganizationId, @PropertyId, @RoleCode,
                    TRUE, TRUE, FALSE, NOW(), @CreatedBySubject
                )
                ON CONFLICT ON CONSTRAINT uq_user_property_access_assignment
                DO NOTHING;
                """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            var property = await connection.QuerySingleOrDefaultAsync<PropertyLookup>(
                new CommandDefinition(
                    """
                    SELECT
                        p.id                AS Id,
                        p.organization_id   AS OrganizationId,
                        p.uid               AS Uid,
                        p.is_archived       AS IsArchived
                    FROM hotel.properties p
                    WHERE p.uid = @PropertyUid;
                    """,
                    new { PropertyUid = propertyUid },
                    transaction,
                    cancellationToken: cancellationToken));

            if (property is null || property.IsArchived)
                throw new InvalidOperationException("Property was not found.");

            await connection.ExecuteAsync(new CommandDefinition(
                insertStaffSql,
                new
                {
                    staff.Uid,
                    staff.Username,
                    staff.NormalizedUsername,
                    staff.Email,
                    staff.NormalizedEmail,
                    staff.FirstName,
                    staff.LastName,
                    staff.PasswordHash,
                    CreatedBy = createdBy
                },
                transaction,
                cancellationToken: cancellationToken));

            var staffPropertyUid = await connection.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    insertPropertySql,
                    new { StaffUid = staff.Uid, PropertyUid = propertyUid },
                    transaction,
                    cancellationToken: cancellationToken));

            var roleRows = await connection.ExecuteAsync(new CommandDefinition(
                insertRoleSql,
                new { StaffPropertyUid = staffPropertyUid, RoleCode = identityRoleCode },
                transaction,
                cancellationToken: cancellationToken));

            if (roleRows == 0)
                throw new InvalidOperationException($"Role '{identityRoleCode}' was not found.");

            var createdBySubject = createdBy?.ToString() ?? staff.Uid.ToString();
            var appUserId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    upsertAppUserSql,
                    new
                    {
                        AppUserUid = Guid.NewGuid(),
                        AuthSubject = staff.Uid.ToString(),
                        staff.Username,
                        staff.FirstName,
                        staff.LastName,
                        staff.Email,
                        CreatedBySubject = createdBySubject
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                insertAccessSql,
                new
                {
                    UserId = appUserId,
                    property.OrganizationId,
                    PropertyId = property.Id,
                    RoleCode = hotelAccessRoleCode,
                    CreatedBySubject = createdBySubject
                },
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
    }
}
