using HMS.Modules.Finance.Api.Contracts;
using HMS.Modules.Finance.Application.Commands.CreateUtilityBill;
using HMS.Modules.Finance.Application.Queries.GetUtilityBills;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/utility-bills")]
[Authorize]
public sealed class PropertyUtilityBillsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUtilityBillsQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] CreateUtilityBillRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateUtilityBillCommand(
            propertyUid,
            request.UtilityTypeUid,
            request.PeriodStart,
            request.PeriodEnd,
            request.PreviousReading,
            request.CurrentReading,
            request.UnitsUsed,
            request.Amount,
            request.Currency,
            request.DueDate,
            request.PaidAt,
            request.ReferenceNumber,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
