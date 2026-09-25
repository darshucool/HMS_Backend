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
}
