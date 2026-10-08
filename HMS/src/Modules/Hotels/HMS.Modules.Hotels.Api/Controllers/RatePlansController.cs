using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.CreateRatePlan;
using HMS.Modules.Hotels.Application.Commands.CreateRatePlanPrice;
using HMS.Modules.Hotels.Application.Commands.DeleteRatePlan;
using HMS.Modules.Hotels.Application.Commands.UpdateRatePlan;
using HMS.Modules.Hotels.Application.Queries.GetRatePlan;
using HMS.Modules.Hotels.Application.Queries.GetRatePlans;
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
    [HttpGet("/api/v1/properties/{propertyUid:guid}/rate-plans")]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRatePlansQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("/api/v1/properties/{propertyUid:guid}/rate-plans")]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] CreateRatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRatePlanCommand(
            propertyUid,
            request.AccommodationTypeUid,
            request.MealPlanUid,
            request.Code,
            request.Name,
            request.PricingBasis,
            request.Currency,
            request.Description,
            request.IsRefundable,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{uid:guid}")]
    public async Task<IActionResult> GetByUid(
        Guid uid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRatePlanQuery(
            uid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{uid:guid}")]
    public async Task<IActionResult> Update(
        Guid uid,
        [FromBody] UpdateRatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateRatePlanCommand(
            uid,
            request.Code,
            request.Name,
            request.PricingBasis,
            request.Currency,
            request.Description,
            request.IsRefundable,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{uid:guid}")]
    public async Task<IActionResult> Delete(
        Guid uid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteRatePlanCommand(
            uid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }

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
