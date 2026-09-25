using HMS.Modules.Finance.Api.Contracts;
using HMS.Modules.Finance.Application.Commands.UpdateUtilityBill;
using HMS.Modules.Finance.Application.Queries.GetUtilityBill;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/utility-bills")]
[Authorize]
public sealed class UtilityBillsController(ISender sender) : ControllerBase
{
    [HttpGet("{utilityBillUid:guid}")]
    public async Task<IActionResult> Get(Guid utilityBillUid, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUtilityBillQuery(
            utilityBillUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{utilityBillUid:guid}")]
    public async Task<IActionResult> Update(
        Guid utilityBillUid,
        [FromBody] UpdateUtilityBillRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateUtilityBillCommand(
            utilityBillUid,
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

        return this.ToActionResult(result);
    }
}