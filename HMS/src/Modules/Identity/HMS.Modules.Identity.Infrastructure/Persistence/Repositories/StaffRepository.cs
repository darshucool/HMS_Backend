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
            string normalizedEmail,
            Guid? propertyUid,
            CancellationToken cancellationToken)
        {
            const string staffSql = """
            SELECT
                su.uid                    AS StaffUid,
                su.username               AS Username,
                su.email                  AS Email,
                su.first_name             AS FirstName,
                su.last_name              AS LastName,
                su.password_hash          AS PasswordHash,
                su.is_active              AS IsActive,
                su.is_locked              AS IsLocked,
                su.failed_login_count     AS FailedLoginCount,
                su.locked_until_utc       AS LockedUntilUtc
            FROM identity.staff_user su
            WHERE su.normalized_email = @NormalizedEmail
            LIMIT 1;
            """;

            const string propertiesSql = """
            SELECT property_uid
            FROM identity.staff_property
            WHERE staff_uid = @StaffUid
              AND is_active = TRUE;
            """;

            const string rolesSql = """
            SELECT DISTINCT sr.code
            FROM identity.staff_property sp
            JOIN identity.staff_user_role sur
              ON sur.staff_property_uid = sp.uid
            JOIN identity.staff_role sr
              ON sr.uid = sur.role_uid
             AND sr.is_active = TRUE
            WHERE sp.staff_uid = @StaffUid
              AND sp.is_active = TRUE
              AND (@PropertyUid IS NULL OR sp.property_uid = @PropertyUid);
            """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            var staff = await connection.QuerySingleOrDefaultAsync<StaffLoginRow>(
                new CommandDefinition(
                    staffSql,
                    new { NormalizedEmail = normalizedEmail },
                    cancellationToken: cancellationToken));

            if (staff is null)
                return null;

            var propertyUids = (await connection.QueryAsync<Guid>(
                new CommandDefinition(
                    propertiesSql,
                    new { staff.StaffUid },
                    cancellationToken: cancellationToken))).AsList();

            if (propertyUid is Guid requiredProperty)
            {
                if (!propertyUids.Contains(requiredProperty))
                    return null;
            }

            var roles = (await connection.QueryAsync<string>(
                new CommandDefinition(
                    rolesSql,
                    new { staff.StaffUid, PropertyUid = propertyUid },
                    cancellationToken: cancellationToken))).AsList();

            var defaultPropertyUid = propertyUid
                ?? (propertyUids.Count > 0 ? propertyUids[0] : Guid.Empty);

            return new StaffLoginRecord
            {
                StaffUid = staff.StaffUid,
                PropertyUid = defaultPropertyUid,
                PropertyUids = [.. propertyUids],
                Username = staff.Username,
                Email = staff.Email,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                PasswordHash = staff.PasswordHash,
                IsActive = staff.IsActive,
                IsLocked = staff.IsLocked,
                FailedLoginCount = staff.FailedLoginCount,
                LockedUntilUtc = staff.LockedUntilUtc,
                Roles = [.. roles]
            };
        }

        public async Task<StaffLoginRecord?> GetByUidAsync(
            Guid staffUid,
            CancellationToken cancellationToken)
        {
            const string sql = """
            SELECT
                su.uid                    AS StaffUid,
                su.username               AS Username,
                su.email                  AS Email,
                su.first_name             AS FirstName,
                su.last_name              AS LastName,
                su.password_hash          AS PasswordHash,
                su.is_active              AS IsActive,
                su.is_locked              AS IsLocked,
                su.failed_login_count     AS FailedLoginCount,
                su.locked_until_utc       AS LockedUntilUtc
            FROM identity.staff_user su
            WHERE su.uid = @StaffUid
            LIMIT 1;
            """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            var staff = await connection.QuerySingleOrDefaultAsync<StaffLoginRow>(
                new CommandDefinition(
                    sql,
                    new { StaffUid = staffUid },
                    cancellationToken: cancellationToken));

            if (staff is null)
                return null;

            var propertyUids = (await connection.QueryAsync<Guid>(
                new CommandDefinition(
                    """
                    SELECT property_uid
                    FROM identity.staff_property
                    WHERE staff_uid = @StaffUid
                      AND is_active = TRUE;
                    """,
                    new { StaffUid = staffUid },
                    cancellationToken: cancellationToken))).AsList();

            var roles = (await connection.QueryAsync<string>(
                new CommandDefinition(
                    """
                    SELECT DISTINCT sr.code
                    FROM identity.staff_property sp
                    JOIN identity.staff_user_role sur
                      ON sur.staff_property_uid = sp.uid
                    JOIN identity.staff_role sr
                      ON sr.uid = sur.role_uid
                     AND sr.is_active = TRUE
                    WHERE sp.staff_uid = @StaffUid
                      AND sp.is_active = TRUE;
                    """,
                    new { StaffUid = staffUid },
                    cancellationToken: cancellationToken))).AsList();

            return new StaffLoginRecord
            {
                StaffUid = staff.StaffUid,
                PropertyUid = propertyUids.Count > 0 ? propertyUids[0] : Guid.Empty,
                PropertyUids = [.. propertyUids],
                Username = staff.Username,
                Email = staff.Email,
                FirstName = staff.FirstName,
                LastName = staff.LastName,
                PasswordHash = staff.PasswordHash,
                IsActive = staff.IsActive,
                IsLocked = staff.IsLocked,
                FailedLoginCount = staff.FailedLoginCount,
                LockedUntilUtc = staff.LockedUntilUtc,
                Roles = [.. roles]
            };
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

        public async Task<ExistingStaffAccount?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    uid         AS Uid,
                    username    AS Username,
                    first_name  AS FirstName,
                    last_name   AS LastName,
                    email       AS Email
                FROM identity.staff_user
                WHERE normalized_email = @NormalizedEmail
                LIMIT 1;
                """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            return await connection.QuerySingleOrDefaultAsync<ExistingStaffAccount>(
                new CommandDefinition(
                    sql,
                    new { NormalizedEmail = normalizedEmail },
                    cancellationToken: cancellationToken));
        }

        public async Task UpdatePasswordAsync(
            Guid staffUid,
            string passwordHash,
            CancellationToken cancellationToken)
        {
            const string sql = """
                UPDATE identity.staff_user
                SET
                    password_hash = @PasswordHash,
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
                        PasswordHash = passwordHash
                    },
                    cancellationToken: cancellationToken));
        }

        public async Task AssignPropertyAsync(
            Guid staffUid,
            Guid propertyUid,
            string identityRoleCode,
            string hotelAccessRoleCode,
            Guid? createdBy,
            CancellationToken cancellationToken)
        {
            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

            await AssignPropertyInternalAsync(
                connection,
                transaction,
                staffUid,
                propertyUid,
                identityRoleCode,
                hotelAccessRoleCode,
                createdBy,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
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

        public async Task EnsureRoleExistsAsync(
            string roleCode,
            string name,
            string? description,
            CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO identity.staff_role(code, name, description, is_active, created_at_utc)
                VALUES (@RoleCode, @Name, @Description, TRUE, NOW())
                ON CONFLICT (code) DO UPDATE
                SET
                    is_active = TRUE,
                    name = COALESCE(EXCLUDED.name, identity.staff_role.name),
                    description = COALESCE(EXCLUDED.description, identity.staff_role.description);
                """;

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        RoleCode = roleCode,
                        Name = name,
                        Description = description
                    },
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
                    p.name              AS Name,
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

            await using var connection =
                await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var transaction =
                await connection.BeginTransactionAsync(cancellationToken);

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

            await AssignPropertyInternalAsync(
                connection,
                transaction,
                staff.Uid,
                propertyUid,
                identityRoleCode,
                hotelAccessRoleCode,
                createdBy,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }

        private static async Task AssignPropertyInternalAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            Guid staffUid,
            Guid propertyUid,
            string identityRoleCode,
            string hotelAccessRoleCode,
            Guid? createdBy,
            CancellationToken cancellationToken)
        {
            var property = await connection.QuerySingleOrDefaultAsync<PropertyLookup>(
                new CommandDefinition(
                    """
                    SELECT
                        p.id                AS Id,
                        p.organization_id   AS OrganizationId,
                        p.uid               AS Uid,
                        p.name              AS Name,
                        p.is_archived       AS IsArchived
                    FROM hotel.properties p
                    WHERE p.uid = @PropertyUid;
                    """,
                    new { PropertyUid = propertyUid },
                    transaction,
                    cancellationToken: cancellationToken));

            if (property is null || property.IsArchived)
                throw new InvalidOperationException("Property was not found.");

            var staffPropertyUid = await connection.ExecuteScalarAsync<Guid?>(
                new CommandDefinition(
                    """
                    INSERT INTO identity.staff_property
                        (staff_uid, property_uid, is_active, assigned_at_utc)
                    VALUES
                        (@StaffUid, @PropertyUid, TRUE, NOW())
                    ON CONFLICT (staff_uid, property_uid)
                    DO UPDATE SET is_active = TRUE
                    RETURNING uid;
                    """,
                    new { StaffUid = staffUid, PropertyUid = propertyUid },
                    transaction,
                    cancellationToken: cancellationToken));

            if (staffPropertyUid is null)
                throw new InvalidOperationException("Could not assign the property.");

            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO identity.staff_user_role
                    (staff_property_uid, role_uid, assigned_at_utc)
                SELECT @StaffPropertyUid, sr.uid, NOW()
                FROM identity.staff_role sr
                WHERE sr.code = @RoleCode
                  AND sr.is_active = TRUE
                ON CONFLICT (staff_property_uid, role_uid) DO NOTHING;
                """,
                new { StaffPropertyUid = staffPropertyUid.Value, RoleCode = identityRoleCode },
                transaction,
                cancellationToken: cancellationToken));

            var roleExists = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    """
                    SELECT EXISTS
                    (
                        SELECT 1
                        FROM identity.staff_user_role sur
                        JOIN identity.staff_role sr ON sr.uid = sur.role_uid
                        WHERE sur.staff_property_uid = @StaffPropertyUid
                          AND sr.code = @RoleCode
                    );
                    """,
                    new { StaffPropertyUid = staffPropertyUid.Value, RoleCode = identityRoleCode },
                    transaction,
                    cancellationToken: cancellationToken));

            if (!roleExists)
                throw new InvalidOperationException($"Role '{identityRoleCode}' was not found.");

            var staff = await connection.QuerySingleAsync<ExistingStaffAccount>(
                new CommandDefinition(
                    """
                    SELECT
                        uid         AS Uid,
                        username    AS Username,
                        first_name  AS FirstName,
                        last_name   AS LastName,
                        email       AS Email
                    FROM identity.staff_user
                    WHERE uid = @StaffUid;
                    """,
                    new { StaffUid = staffUid },
                    transaction,
                    cancellationToken: cancellationToken));

            var createdBySubject = createdBy?.ToString() ?? staffUid.ToString();
            var hasDefault = await connection.ExecuteScalarAsync<bool>(
                new CommandDefinition(
                    """
                    SELECT EXISTS
                    (
                        SELECT 1
                        FROM hotel.app_users u
                        JOIN hotel.user_property_access upa ON upa.user_id = u.id
                        WHERE u.auth_subject = @AuthSubject
                          AND upa.is_default_property = TRUE
                          AND upa.is_archived = FALSE
                    );
                    """,
                    new { AuthSubject = staffUid.ToString() },
                    transaction,
                    cancellationToken: cancellationToken));

            var appUserId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    """
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
                    """,
                    new
                    {
                        AppUserUid = Guid.NewGuid(),
                        AuthSubject = staffUid.ToString(),
                        staff.Username,
                        staff.FirstName,
                        staff.LastName,
                        staff.Email,
                        CreatedBySubject = createdBySubject
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO hotel.user_property_access
                (
                    user_id, organization_id, property_id, role_code,
                    is_default_property, is_active, is_archived, creation_date, created_by
                )
                VALUES
                (
                    @UserId, @OrganizationId, @PropertyId, @RoleCode,
                    @IsDefault, TRUE, FALSE, NOW(), @CreatedBySubject
                )
                ON CONFLICT ON CONSTRAINT uq_user_property_access_assignment
                DO NOTHING;
                """,
                new
                {
                    UserId = appUserId,
                    property.OrganizationId,
                    PropertyId = property.Id,
                    RoleCode = hotelAccessRoleCode,
                    IsDefault = !hasDefault,
                    CreatedBySubject = createdBySubject
                },
                transaction,
                cancellationToken: cancellationToken));
        }

        private sealed class StaffLoginRow
        {
            public Guid StaffUid { get; init; }
            public string Username { get; init; } = string.Empty;
            public string? Email { get; init; }
            public string FirstName { get; init; } = string.Empty;
            public string? LastName { get; init; }
            public string PasswordHash { get; init; } = string.Empty;
            public bool IsActive { get; init; }
            public bool IsLocked { get; init; }
            public int FailedLoginCount { get; init; }
            public DateTimeOffset? LockedUntilUtc { get; init; }
        }
    }
}

