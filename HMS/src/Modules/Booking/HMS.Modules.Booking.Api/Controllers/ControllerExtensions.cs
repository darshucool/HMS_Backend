using System.Security.Claims;
using HMS.Modules.Booking.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Booking.Api.Controllers;

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
        BookingResult<T> result)
    {
        return result.Status switch
        {
            BookingResultStatus.Success => controller.Ok(result.Value),
            BookingResultStatus.Validation => controller.BadRequest(new { error = result.Error }),
            BookingResultStatus.NotFound => controller.NotFound(new { error = result.Error }),
            BookingResultStatus.Conflict => controller.Conflict(new { error = result.Error }),
            BookingResultStatus.Forbidden => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                new { error = result.Error }),
            _ => controller.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
