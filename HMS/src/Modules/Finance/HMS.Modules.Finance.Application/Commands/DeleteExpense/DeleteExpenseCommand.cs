using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(
    Guid ExpenseUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<bool>>;

public sealed class DeleteExpenseCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IExpenseRepository expenseRepository)
    : IRequestHandler<DeleteExpenseCommand, FinanceResult<bool>>
{
    public async Task<FinanceResult<bool>> Handle(
        DeleteExpenseCommand request,
        CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByUidAsync(request.ExpenseUid, cancellationToken);
        if (expense is null || expense.IsArchived)
            return FinanceResult<bool>.NotFound("Expense was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject, expense.PropertyUid, true, cancellationToken))
        {
            return FinanceResult<bool>.Forbidden("You cannot delete this expense.");
        }

        await expenseRepository.DeleteAsync(expense.Id, request.ActorSubject, cancellationToken);
        return FinanceResult<bool>.Success(true);
    }
}