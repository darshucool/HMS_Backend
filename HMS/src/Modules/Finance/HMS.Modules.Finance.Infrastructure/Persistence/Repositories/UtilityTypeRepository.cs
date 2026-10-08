using Dapper;
using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.DTOs;

namespace HMS.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class UtilityTypeRepository(IFinanceDbConnectionFactory connectionFactory)
    : IUtilityTypeRepository
{
    public async Task<IReadOnlyList<UtilityTypeDto>> GetByOrganizationIdAsync(
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid             AS "Uid",
                code            AS "Code",
                name            AS "Name",
                unit_of_measure AS "UnitOfMeasure",
                is_metered      AS "IsMetered",
                is_active       AS "IsActive"
            FROM hotel.utility_types
            WHERE organization_id = @OrganizationId
              AND is_archived = false
            ORDER BY name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<UtilityTypeRow>(
            new CommandDefinition(sql, new { OrganizationId = organizationId }, cancellationToken: cancellationToken));
        return rows.Select(ToDto).ToList();
    }

    public async Task<UtilityTypeContext?> GetByUidAsync(
        Guid utilityTypeUid,
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                uid             AS "Uid",
                organization_id AS "OrganizationId",
                is_archived     AS "IsArchived"
            FROM hotel.utility_types
            WHERE uid = @UtilityTypeUid
              AND organization_id = @OrganizationId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<UtilityTypeContext>(
            new CommandDefinition(
                sql,
                new { UtilityTypeUid = utilityTypeUid, OrganizationId = organizationId },
                cancellationToken: cancellationToken));
    }

    public async Task<UtilityTypeDto?> GetDetailAsync(
        Guid utilityTypeUid,
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid             AS "Uid",
                code            AS "Code",
                name            AS "Name",
                unit_of_measure AS "UnitOfMeasure",
                is_metered      AS "IsMetered",
                is_active       AS "IsActive"
            FROM hotel.utility_types
            WHERE uid = @UtilityTypeUid
              AND organization_id = @OrganizationId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<UtilityTypeRow>(
            new CommandDefinition(
                sql,
                new { UtilityTypeUid = utilityTypeUid, OrganizationId = organizationId },
                cancellationToken: cancellationToken));
        return row is null ? null : ToDto(row);
    }

    public async Task<bool> CodeExistsAsync(
        long organizationId,
        string code,
        Guid? excludeUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.utility_types
                WHERE organization_id = @OrganizationId
                  AND code = @Code
                  AND (@ExcludeUid IS NULL OR uid <> @ExcludeUid)
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { OrganizationId = organizationId, Code = code, ExcludeUid = excludeUid },
            cancellationToken: cancellationToken));
    }

    public async Task<Guid> InsertAsync(
        long organizationId,
        string code,
        string name,
        string? unitOfMeasure,
        bool isMetered,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.utility_types
            (
                uid, organization_id, code, name, unit_of_measure, is_metered,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @Code, @Name, @UnitOfMeasure, @IsMetered,
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
                UnitOfMeasure = unitOfMeasure,
                IsMetered = isMetered,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task UpdateAsync(
        long utilityTypeId,
        string code,
        string name,
        string? unitOfMeasure,
        bool isMetered,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.utility_types
            SET code = @Code,
                name = @Name,
                unit_of_measure = @UnitOfMeasure,
                is_metered = @IsMetered,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @UtilityTypeId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                UtilityTypeId = utilityTypeId,
                Code = code,
                Name = name,
                UnitOfMeasure = unitOfMeasure,
                IsMetered = isMetered,
                ModifiedDate = DateTimeOffset.UtcNow,
                ModifiedBy = actorSubject
            },
            cancellationToken: cancellationToken));
    }

    public async Task ArchiveAsync(long utilityTypeId, string actorSubject, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.utility_types
            SET is_active = false,
                is_archived = true,
                modified_date = CURRENT_TIMESTAMP,
                modified_by = @ActorSubject
            WHERE id = @UtilityTypeId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { UtilityTypeId = utilityTypeId, ActorSubject = actorSubject },
            cancellationToken: cancellationToken));
    }

    private static UtilityTypeDto ToDto(UtilityTypeRow row) => new(
        row.Uid,
        row.Code,
        row.Name,
        row.UnitOfMeasure,
        row.IsMetered,
        row.IsActive);

    private sealed class UtilityTypeRow
    {
        public Guid Uid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? UnitOfMeasure { get; init; }
        public bool IsMetered { get; init; }
        public bool IsActive { get; init; }
    }
}
