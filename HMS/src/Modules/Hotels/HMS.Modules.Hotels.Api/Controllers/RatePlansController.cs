using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.CreateRatePlanPrice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/rate-plans")]
[Authorize]
public sealed class RatePlansController(ISender sender) : ControllerBase
{
    [HttpPost("{uid:guid}/prices")]
    public async Task<IActionResult> CreatePrice(
        Guid uid,
        [FromBody] CreateRatePlanPriceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRatePlanPriceCommand(
            uid,
            request.StartDate,
            request.EndDate,
            request.DayOfWeek,
            request.AdultRate,
            request.ChildRate,
            request.UnitRate,
            request.MinimumStay,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
