using Dapper;
using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.DTOs;

namespace HMS.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class IncomeCategoryRepository(IFinanceDbConnectionFactory connectionFactory)
    : IIncomeCategoryRepository
{
    public async Task<IReadOnlyList<IncomeCategoryDto>> ListAsync(
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid         AS "Uid",
                code        AS "Code",
                name        AS "Name",
                is_active   AS "IsActive"
            FROM hotel.income_categories
            WHERE organization_id = @OrganizationId
              AND is_archived = false
            ORDER BY name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<IncomeCategoryRow>(
            new CommandDefinition(sql, new { OrganizationId = organizationId }, cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    public async Task<IncomeCategoryDto?> GetDetailAsync(
        Guid categoryUid,
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid         AS "Uid",
                code        AS "Code",
                name        AS "Name",
                is_active   AS "IsActive"
            FROM hotel.income_categories
            WHERE uid = @CategoryUid
              AND organization_id = @OrganizationId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<IncomeCategoryRow>(
            new CommandDefinition(
                sql,
                new { CategoryUid = categoryUid, OrganizationId = organizationId },
                cancellationToken: cancellationToken));

        return row is null ? null : ToDto(row);
    }

    public async Task<IncomeCategoryContext?> GetContextAsync(
        Guid categoryUid,
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id          AS "Id",
                uid         AS "Uid",
                is_archived AS "IsArchived"
            FROM hotel.income_categories
            WHERE uid = @CategoryUid
              AND organization_id = @OrganizationId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<IncomeCategoryContextRow>(
            new CommandDefinition(
                sql,
                new { CategoryUid = categoryUid, OrganizationId = organizationId },
                cancellationToken: cancellationToken));

        return row is null ? null : new IncomeCategoryContext(row.Id, row.Uid, row.IsArchived);
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
                FROM hotel.income_categories
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
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.income_categories
            (
                uid, organization_id, code, name, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @Code, @Name, @IsActive, false, @CreationDate, @CreatedBy
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
                IsActive = isActive,
                CreationDate = DateTime.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task UpdateAsync(
        long categoryId,
        string code,
        string name,
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.income_categories
            SET code = @Code,
                name = @Name,
                is_active = @IsActive,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @CategoryId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                CategoryId = categoryId,
                Code = code,
                Name = name,
                IsActive = isActive,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = actorSubject
            },
            cancellationToken: cancellationToken));
    }

    public async Task ArchiveAsync(long categoryId, string actorSubject, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.income_categories
            SET is_archived = true,
                is_active = false,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @CategoryId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                CategoryId = categoryId,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = actorSubject
            },
            cancellationToken: cancellationToken));
    }

    private static IncomeCategoryDto ToDto(IncomeCategoryRow row) =>
        new(row.Uid, row.Code, row.Name, row.IsActive);

    private sealed class IncomeCategoryRow
    {
        public Guid Uid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }

    private sealed class IncomeCategoryContextRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public bool IsArchived { get; init; }
    }
}
