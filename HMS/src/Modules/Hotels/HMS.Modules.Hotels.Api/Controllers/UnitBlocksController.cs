using HMS.Modules.Hotels.Application.Commands.DeleteUnitBlock;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/unit-blocks")]
[Authorize]
public sealed class UnitBlocksController(ISender sender) : ControllerBase
{
    [HttpDelete("{uid:guid}")]
    public async Task<IActionResult> Delete(
        Guid uid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUnitBlockCommand(
            uid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }
}
