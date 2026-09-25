using HMS.Modules.Finance.Application.Queries.GetExpenseCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/expense-categories")]
[Authorize]
public sealed class PropertyExpenseCategoriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetExpenseCategoriesQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}
