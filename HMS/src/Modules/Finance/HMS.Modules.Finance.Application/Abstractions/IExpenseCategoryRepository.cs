using HMS.Modules.Finance.Application.DTOs;

namespace HMS.Modules.Finance.Application.Abstractions;

public interface IExpenseCategoryRepository
{
    Task<IReadOnlyList<ExpenseCategoryDto>> GetByOrganizationIdAsync(
        long organizationId,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(long organizationId, string code, CancellationToken cancellationToken);

    Task<long?> GetIdAsync(Guid categoryUid, long organizationId, CancellationToken cancellationToken);

    Task<Guid> InsertAsync(
        long organizationId,
        long? parentId,
        string code,
        string name,
        string expenseGroup,
        string actorSubject,
        CancellationToken cancellationToken);
}
