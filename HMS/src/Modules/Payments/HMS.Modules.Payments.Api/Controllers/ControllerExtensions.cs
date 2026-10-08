using System.Security.Claims;
using HMS.Modules.Payments.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Payments.Api.Controllers;

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
        PaymentResult<T> result)
    {
        return result.Status switch
        {
            PaymentResultStatus.Success => controller.Ok(result.Value),
            PaymentResultStatus.Validation => controller.BadRequest(new { error = result.Error }),
            PaymentResultStatus.NotFound => controller.NotFound(new { error = result.Error }),
            PaymentResultStatus.Conflict => controller.Conflict(new { error = result.Error }),
            PaymentResultStatus.Forbidden => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                new { error = result.Error }),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
