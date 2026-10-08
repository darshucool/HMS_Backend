using Dapper;
using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.DTOs;

namespace HMS.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class ExpenseCategoryRepository(IFinanceDbConnectionFactory connectionFactory)
    : IExpenseCategoryRepository
{
    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetByOrganizationIdAsync(
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.uid           AS "Uid",
                p.uid           AS "ParentUid",
                c.code          AS "Code",
                c.name          AS "Name",
                c.expense_group AS "ExpenseGroup",
                c.is_active     AS "IsActive"
            FROM hotel.expense_categories c
            LEFT JOIN hotel.expense_categories p ON p.id = c.parent_id
            WHERE c.organization_id = @OrganizationId
              AND c.is_archived = false
            ORDER BY c.expense_group, c.name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<ExpenseCategoryDto>(
            new CommandDefinition(sql, new { OrganizationId = organizationId }, cancellationToken: cancellationToken));

        return items.AsList();
    }

    public async Task<bool> CodeExistsAsync(long organizationId, string code, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.expense_categories
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

    public async Task<long?> GetIdAsync(
        Guid categoryUid,
        long organizationId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id
            FROM hotel.expense_categories
            WHERE uid = @CategoryUid
              AND organization_id = @OrganizationId
              AND is_archived = false
              AND is_active = true;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<long?>(new CommandDefinition(
            sql,
            new { CategoryUid = categoryUid, OrganizationId = organizationId },
            cancellationToken: cancellationToken));
    }

    public async Task<Guid> InsertAsync(
        long organizationId,
        long? parentId,
        string code,
        string name,
        string expenseGroup,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.expense_categories
            (
                uid, organization_id, parent_id, code, name, expense_group,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @ParentId, @Code, @Name, @ExpenseGroup,
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
                ParentId = parentId,
                Code = code,
                Name = name,
                ExpenseGroup = expenseGroup,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }
}
