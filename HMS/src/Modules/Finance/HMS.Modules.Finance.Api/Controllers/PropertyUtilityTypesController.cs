using HMS.Modules.Finance.Api.Contracts;
using HMS.Modules.Finance.Application.Commands.CreateUtilityType;
using HMS.Modules.Finance.Application.Commands.DeleteUtilityType;
using HMS.Modules.Finance.Application.Commands.UpdateUtilityType;
using HMS.Modules.Finance.Application.Queries.GetUtilityType;
using HMS.Modules.Finance.Application.Queries.GetUtilityTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/utility-types")]
[Authorize]
public sealed class PropertyUtilityTypesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUtilityTypesQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] UpsertUtilityTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateUtilityTypeCommand(
            propertyUid,
            request.Code,
            request.Name,
            request.UnitOfMeasure,
            request.IsMetered,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{utilityTypeUid:guid}")]
    public async Task<IActionResult> GetOne(
        Guid propertyUid,
        Guid utilityTypeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUtilityTypeQuery(
            propertyUid,
            utilityTypeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{utilityTypeUid:guid}")]
    public async Task<IActionResult> Update(
        Guid propertyUid,
        Guid utilityTypeUid,
        [FromBody] UpsertUtilityTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateUtilityTypeCommand(
            propertyUid,
            utilityTypeUid,
            request.Code,
            request.Name,
            request.UnitOfMeasure,
            request.IsMetered,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{utilityTypeUid:guid}")]
    public async Task<IActionResult> Delete(
        Guid propertyUid,
        Guid utilityTypeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUtilityTypeCommand(
            propertyUid,
            utilityTypeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }
}
