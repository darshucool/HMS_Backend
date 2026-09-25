using System.Security.Claims;
using HMS.Modules.Finance.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Finance.Api.Controllers;

internal static class ControllerExtensions
{
    public static string GetRequiredSubject(this ClaimsPrincipal user) =>
        user.FindFirstValue("sub")
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("The access token does not contain a subject claim.");

    public static bool IsPlatformAdmin(this ClaimsPrincipal user) =>
        user.IsInRole("PLATFORM_ADMIN")
        || user.HasClaim("role", "PLATFORM_ADMIN")
        || user.HasClaim(ClaimTypes.Role, "PLATFORM_ADMIN");

    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        FinanceResult<T> result)
    {
        return result.Status switch
        {
            FinanceResultStatus.Success => controller.Ok(result.Value),
            FinanceResultStatus.Validation => controller.BadRequest(new { error = result.Error }),
            FinanceResultStatus.NotFound => controller.NotFound(new { error = result.Error }),
            FinanceResultStatus.Conflict => controller.Conflict(new { error = result.Error }),
            FinanceResultStatus.Forbidden => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                new { error = result.Error }),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
