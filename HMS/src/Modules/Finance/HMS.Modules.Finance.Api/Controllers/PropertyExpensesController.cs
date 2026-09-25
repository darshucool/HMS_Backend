using HMS.Modules.Finance.Api.Contracts;
using HMS.Modules.Finance.Application.Commands.CreateExpense;
using HMS.Modules.Finance.Application.Queries.GetExpenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/expenses")]
[Authorize]
public sealed class PropertyExpensesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetExpensesQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateExpenseCommand(
            propertyUid,
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

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
