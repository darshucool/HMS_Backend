using HMS.Modules.Identity.Api.Authorization;
using HMS.Modules.Identity.Api.Contracts;
using HMS.Modules.Identity.Application.Commands.RemovePropertyAdmin;
using HMS.Modules.Identity.Application.Commands.UpdatePropertyAdmin;
using HMS.Modules.Identity.Application.Queries.GetPropertyAdmin;
using HMS.Modules.Identity.Application.Queries.GetPropertyAdmins;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Identity.Api.Controllers;

[ApiController]
[Authorize(Policy = IdentityPolicies.PlatformAdmin)]
[Route("api/v1/properties/{propertyUid:guid}/admins")]
public sealed class PropertyAdminsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPropertyAdminsQuery(propertyUid), cancellationToken);
        return result.IsSuccessful
            ? Ok(result.Data)
            : this.ToAuthResult(false, null, result.ErrorCode, result.ErrorMessage);
    }

    [HttpGet("{adminId:guid}")]
    public async Task<IActionResult> GetOne(
        Guid propertyUid,
        Guid adminId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPropertyAdminQuery(propertyUid, adminId), cancellationToken);
        return result.IsSuccessful
            ? Ok(result.Data)
            : this.ToAuthResult(false, null, result.ErrorCode, result.ErrorMessage);
    }

    [HttpPut("{adminId:guid}")]
    public async Task<IActionResult> Update(
        Guid propertyUid,
        Guid adminId,
        [FromBody] UpdatePropertyAdminRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePropertyAdminCommand(
            propertyUid,
            adminId,
            request.FirstName,
            request.LastName,
            request.IsActive), cancellationToken);

        return result.IsSuccessful
            ? Ok(result.Data)
            : this.ToAuthResult(false, null, result.ErrorCode, result.ErrorMessage);
    }

    [HttpDelete("{adminId:guid}")]
    public async Task<IActionResult> Delete(
        Guid propertyUid,
        Guid adminId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemovePropertyAdminCommand(propertyUid, adminId), cancellationToken);
        return result.IsSuccessful
            ? NoContent()
            : this.ToAuthResult(false, null, result.ErrorCode, result.ErrorMessage);
    }
}
