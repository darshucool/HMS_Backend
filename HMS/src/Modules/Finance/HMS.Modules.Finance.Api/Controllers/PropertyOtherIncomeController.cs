using HMS.Modules.Finance.Api.Contracts;
using HMS.Modules.Finance.Application.Commands.CreateOtherIncome;
using HMS.Modules.Finance.Application.Commands.DeleteOtherIncome;
using HMS.Modules.Finance.Application.Commands.UpdateOtherIncome;
using HMS.Modules.Finance.Application.Queries.GetOtherIncome;
using HMS.Modules.Finance.Application.Queries.GetOtherIncomeByUid;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/other-income")]
[Authorize]
public sealed class PropertyOtherIncomeController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOtherIncomeQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{otherIncomeUid:guid}")]
    public async Task<IActionResult> GetOne(
        Guid propertyUid,
        Guid otherIncomeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOtherIncomeByUidQuery(
            propertyUid,
            otherIncomeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{otherIncomeUid:guid}")]
    public async Task<IActionResult> Update(
        Guid propertyUid,
        Guid otherIncomeUid,
        [FromBody] CreateOtherIncomeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateOtherIncomeCommand(
            propertyUid,
            otherIncomeUid,
            request.IncomeCategoryUid,
            request.BookingUid,
            request.IncomeDate,
            request.Description,
            request.Amount,
            request.Currency,
            request.PaymentMethod,
            request.ReferenceNumber,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{otherIncomeUid:guid}")]
    public async Task<IActionResult> Delete(
        Guid propertyUid,
        Guid otherIncomeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteOtherIncomeCommand(
            propertyUid,
            otherIncomeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] CreateOtherIncomeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOtherIncomeCommand(
            propertyUid,
            request.IncomeCategoryUid,
            request.BookingUid,
            request.IncomeDate,
            request.Description,
            request.Amount,
            request.Currency,
            request.PaymentMethod,
            request.ReferenceNumber,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
