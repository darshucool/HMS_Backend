using HMS.Modules.Finance.Api.Contracts;
using HMS.Modules.Finance.Application.Commands.CreateIncomeCategory;
using HMS.Modules.Finance.Application.Commands.DeleteIncomeCategory;
using HMS.Modules.Finance.Application.Commands.UpdateIncomeCategory;
using HMS.Modules.Finance.Application.Queries.GetIncomeCategories;
using HMS.Modules.Finance.Application.Queries.GetIncomeCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/income-categories")]
[Authorize]
public sealed class PropertyIncomeCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetIncomeCategoriesQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{incomeCategoryUid:guid}")]
    public async Task<IActionResult> GetOne(
        Guid propertyUid,
        Guid incomeCategoryUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetIncomeCategoryQuery(
            propertyUid,
            incomeCategoryUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] UpsertIncomeCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateIncomeCategoryCommand(
            propertyUid,
            request.Code,
            request.Name,
            request.IsActive,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPut("{incomeCategoryUid:guid}")]
    public async Task<IActionResult> Update(
        Guid propertyUid,
        Guid incomeCategoryUid,
        [FromBody] UpsertIncomeCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateIncomeCategoryCommand(
            propertyUid,
            incomeCategoryUid,
            request.Code,
            request.Name,
            request.IsActive,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{incomeCategoryUid:guid}")]
    public async Task<IActionResult> Delete(
        Guid propertyUid,
        Guid incomeCategoryUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteIncomeCategoryCommand(
            propertyUid,
            incomeCategoryUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }
}
