using HMS.Modules.Finance.Application.DTOs;

namespace HMS.Modules.Finance.Application.Abstractions;

public interface IExpenseCategoryRepository
{
    Task<IReadOnlyList<ExpenseCategoryDto>> GetByOrganizationIdAsync(
        long organizationId,
        CancellationToken cancellationToken);
}
