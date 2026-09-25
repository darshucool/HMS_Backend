using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetExpenses;

public sealed record GetExpensesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IReadOnlyList<ExpenseDto>>>;

public sealed class GetExpensesQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IExpenseRepository expenseRepository)
    : IRequestHandler<GetExpensesQuery, FinanceResult<IReadOnlyList<ExpenseDto>>>
{
    public async Task<FinanceResult<IReadOnlyList<ExpenseDto>>> Handle(
        GetExpensesQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return FinanceResult<IReadOnlyList<ExpenseDto>>.Forbidden(
                "You cannot view expenses for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<IReadOnlyList<ExpenseDto>>.NotFound("Property was not found.");

        var items = await expenseRepository.GetByPropertyUidAsync(request.PropertyUid, cancellationToken);
        return FinanceResult<IReadOnlyList<ExpenseDto>>.Success(items);
    }
}
