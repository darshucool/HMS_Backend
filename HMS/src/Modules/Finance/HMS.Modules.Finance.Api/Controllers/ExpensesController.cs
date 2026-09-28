using HMS.Modules.Finance.Api.Contracts;
using HMS.Modules.Finance.Application.Commands.DeleteExpense;
using HMS.Modules.Finance.Application.Commands.UpdateExpense;
using HMS.Modules.Finance.Application.Queries.GetExpense;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/expenses")]
[Authorize]
public sealed class ExpensesController(ISender sender) : ControllerBase
{
    [HttpGet("{expenseUid:guid}")]
    public async Task<IActionResult> Get(Guid expenseUid, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetExpenseQuery(
            expenseUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{expenseUid:guid}")]
    public async Task<IActionResult> Update(
        Guid expenseUid,
        [FromBody] UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateExpenseCommand(
            expenseUid,
            request.ExpenseCategoryUid,
            request.SupplierUid,
            request.BookingUid,
            request.ExpenseDate,
            request.Description,
            request.Amount,
            request.Currency,
            request.PaymentMethod,
            request.ReferenceNumber,
            request.ReceiptUrl,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{expenseUid:guid}")]
    public async Task<IActionResult> Delete(Guid expenseUid, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteExpenseCommand(
            expenseUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }
}