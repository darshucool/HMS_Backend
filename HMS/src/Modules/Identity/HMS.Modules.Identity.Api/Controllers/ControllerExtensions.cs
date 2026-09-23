using System.Security.Claims;
using HMS.Modules.Identity.Application.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Identity.Api.Controllers;

internal static class ControllerExtensions
{
    public static string GetRequiredSubject(this ClaimsPrincipal user) =>
        user.FindFirstValue("sub")
        ?? user.FindFirstValue("staff_uid")
        ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("The access token does not contain a subject claim.");

    public static Guid? GetStaffUid(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("staff_uid")
                    ?? user.FindFirstValue("sub")
                    ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var uid) ? uid : null;
    }

    public static Guid? GetPropertyUid(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("property_uid"), out var uid) ? uid : null;

    public static bool IsPlatformAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(IdentityRoles.PlatformAdmin)
        || user.HasClaim("role", IdentityRoles.PlatformAdmin)
        || user.HasClaim(ClaimTypes.Role, IdentityRoles.PlatformAdmin);

    public static bool IsHotelAdmin(this ClaimsPrincipal user) =>
        user.IsInRole(IdentityRoles.HotelAdmin)
        || user.IsInRole(IdentityRoles.Manager)
        || user.HasClaim("role", IdentityRoles.HotelAdmin)
        || user.HasClaim("role", IdentityRoles.Manager)
        || user.HasClaim(ClaimTypes.Role, IdentityRoles.HotelAdmin)
        || user.HasClaim(ClaimTypes.Role, IdentityRoles.Manager);

    public static IActionResult ToAuthResult(
        this ControllerBase controller,
        bool isSuccessful,
        object? data,
        string? errorCode,
        string? errorMessage)
    {
        if (isSuccessful)
            return controller.Ok(data);

        return errorCode switch
        {
            "validation_error" => controller.BadRequest(new { errorCode, errorMessage }),
            "account_locked" => controller.StatusCode(423, new { errorCode, errorMessage }),
            "account_inactive" => controller.Unauthorized(new { errorCode, errorMessage }),
            "not_found" => controller.NotFound(new { errorCode, errorMessage }),
            "conflict" => controller.Conflict(new { errorCode, errorMessage }),
            "forbidden" => controller.StatusCode(403, new { errorCode, errorMessage }),
            _ => controller.Unauthorized(new { errorCode, errorMessage })
        };
    }
}
