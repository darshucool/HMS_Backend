using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetExpense;

public sealed record GetExpenseQuery(
    Guid ExpenseUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<ExpenseDto>>;

public sealed class GetExpenseQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IExpenseRepository expenseRepository)
    : IRequestHandler<GetExpenseQuery, FinanceResult<ExpenseDto>>
{
    public async Task<FinanceResult<ExpenseDto>> Handle(
        GetExpenseQuery request,
        CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByUidAsync(request.ExpenseUid, cancellationToken);
        if (expense is null || expense.IsArchived)
            return FinanceResult<ExpenseDto>.NotFound("Expense was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                expense.PropertyUid,
                false,
                cancellationToken))
        {
            return FinanceResult<ExpenseDto>.Forbidden("You cannot view this expense.");
        }

        var detail = await expenseRepository.GetDetailByUidAsync(request.ExpenseUid, cancellationToken);
        return detail is null
            ? FinanceResult<ExpenseDto>.NotFound("Expense was not found.")
            : FinanceResult<ExpenseDto>.Success(detail);
    }
}
