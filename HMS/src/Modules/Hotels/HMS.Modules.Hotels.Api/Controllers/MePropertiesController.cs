using HMS.Modules.Hotels.Application.Queries.GetMyProperties;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/me/properties")]
[Authorize]
public sealed class MePropertiesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetMyPropertiesQuery(User.GetRequiredSubject()),
            cancellationToken);

        return this.ToActionResult(result);
    }
}

