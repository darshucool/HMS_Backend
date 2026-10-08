using Dapper;
using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.DTOs;

namespace HMS.Modules.Staff.Infrastructure.Persistence.Repositories;

public sealed class StaffRoleRepository(IStaffDbConnectionFactory connectionFactory)
    : IStaffRoleRepository
{
    public async Task<bool> CodeExistsAsync(long organizationId, string code, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.staff_roles
                WHERE organization_id = @OrganizationId
                  AND code = @Code
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { OrganizationId = organizationId, Code = code },
            cancellationToken: cancellationToken));
    }

    public async Task<StaffRoleDto> InsertAsync(
        long organizationId,
        string code,
        string name,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.staff_roles
            (
                uid, organization_id, code, name,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @Code, @Name,
                true, false, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Uid = uid,
                OrganizationId = organizationId,
                Code = code,
                Name = name,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return new StaffRoleDto(uid, name);
    }

    public async Task<IReadOnlyList<StaffRoleDto>> ListAsync(
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid  AS "Id",
                name AS "Name"
            FROM hotel.staff_roles
            WHERE organization_id = @OrganizationId
              AND is_archived = false
              AND is_active = true
            ORDER BY name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<StaffRoleRow>(new CommandDefinition(
            sql,
            new { OrganizationId = organizationId },
            cancellationToken: cancellationToken));
        return rows.Select(row => new StaffRoleDto(row.Id, row.Name)).ToList();
    }

    private sealed class StaffRoleRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    public async Task<StaffRoleDeleteStatus> ArchiveAsync(
        Guid staffRoleUid,
        long organizationId,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.staff_roles r
            SET is_active = false,
                is_archived = true,
                modified_date = CURRENT_TIMESTAMP,
                modified_by = @ActorSubject
            WHERE r.uid = @StaffRoleUid
              AND r.organization_id = @OrganizationId
              AND r.is_archived = false
              AND NOT EXISTS
              (
                  SELECT 1
                  FROM hotel.staff_members m
                  WHERE m.staff_role_id = r.id
                    AND m.is_archived = false
              );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var updated = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                StaffRoleUid = staffRoleUid,
                OrganizationId = organizationId,
                ActorSubject = actorSubject
            },
            cancellationToken: cancellationToken));

        if (updated > 0)
            return StaffRoleDeleteStatus.Deleted;

        const string existsSql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.staff_members m
                JOIN hotel.staff_roles r ON r.id = m.staff_role_id
                WHERE r.uid = @StaffRoleUid
                  AND r.organization_id = @OrganizationId
                  AND m.is_archived = false
            );
            """;

        var inUse = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            existsSql,
            new { StaffRoleUid = staffRoleUid, OrganizationId = organizationId },
            cancellationToken: cancellationToken));

        return inUse ? StaffRoleDeleteStatus.InUse : StaffRoleDeleteStatus.NotFound;
    }
}
