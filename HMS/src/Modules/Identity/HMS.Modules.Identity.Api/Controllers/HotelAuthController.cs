using HMS.Modules.Identity.Api.Authorization;
using HMS.Modules.Identity.Api.Contracts;
using HMS.Modules.Identity.Application.Authentication;
using HMS.Modules.Identity.Application.Commands.InvitePropertyAdmin;
using HMS.Modules.Identity.Application.Commands.LoginStaff;
using HMS.Modules.Identity.Application.Commands.LoginSuperAdmin;
using HMS.Modules.Identity.Application.Commands.RegisterStaff;
using HMS.Modules.Identity.Application.Commands.ResetAdminCredentials;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace HMS.Modules.Identity.Api.Controllers;

[ApiController]
[Route("api/hotel/auth")]
public sealed class HotelAuthController(
    ISender sender,
    IConfiguration configuration) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("super-admin/login")]
    [ProducesResponseType(typeof(StaffLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginSuperAdmin(
        [FromBody] SuperAdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginSuperAdminCommand(request.Email, request.Password),
            cancellationToken);

        return this.ToAuthResult(
            result.IsSuccessful,
            result.Data,
            result.ErrorCode,
            result.ErrorMessage);
    }

    [AllowAnonymous]
    [HttpPost("admin/login")]
    [ProducesResponseType(typeof(StaffLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public Task<IActionResult> LoginAdmin(
        [FromBody] StaffLoginRequest request,
        CancellationToken cancellationToken) =>
        LoginPropertyUser(
            request,
            IdentityRoles.AdminLoginRoles,
            cancellationToken);

    [Authorize(Policy = IdentityPolicies.PlatformAdmin)]
    [HttpPost("admin/register")]
    [ProducesResponseType(typeof(RegisterStaffResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterAdmin(
        [FromBody] RegisterAdminRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterStaffCommand(
                request.PropertyUid,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Email,
                IdentityRoles.HotelAdmin,
                User.GetRequiredSubject(),
                User.GetStaffUid(),
                User.GetPropertyUid(),
                true,
                true),
            cancellationToken);

        return ToRegisterResult(result);
    }

    [Authorize(Policy = IdentityPolicies.PlatformAdmin)]
    [HttpPost("admin/invite")]
    [ProducesResponseType(typeof(RegisterStaffResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InviteAdmin(
        [FromBody] InvitePropertyAdminRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new InvitePropertyAdminCommand(
                request.PropertyUid,
                request.Email,
                request.FirstName,
                request.LastName,
                User.GetStaffUid(),
                configuration["App:AdminLoginUrl"] ?? "http://localhost:3000/admin/login"),
            cancellationToken);

        return ToRegisterResult(result);
    }

    [Authorize]
    [HttpPost("admin/credentials")]
    [ProducesResponseType(typeof(ResetAdminCredentialsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResetAdminCredentials(
        [FromBody] ResetAdminCredentialsRequest request,
        CancellationToken cancellationToken)
    {
        var staffUid = User.GetStaffUid();
        if (staffUid is null)
        {
            return this.ToAuthResult(
                false,
                null,
                "forbidden",
                "The access token does not identify a staff user.");
        }

        var result = await sender.Send(
            new ResetAdminCredentialsCommand(
                staffUid.Value,
                request.Password,
                User.IsHotelAdmin()),
            cancellationToken);

        if (result.IsSuccessful)
            return Ok(result.Data);

        return this.ToAuthResult(
            false,
            null,
            result.ErrorCode,
            result.ErrorMessage);
    }

    [AllowAnonymous]
    [HttpPost("staff/login")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(StaffLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public Task<IActionResult> LoginStaff(
        [FromBody] StaffLoginRequest request,
        CancellationToken cancellationToken) =>
        LoginPropertyUser(
            request,
            IdentityRoles.StaffLoginRoles,
            cancellationToken);

    [Authorize]
    [HttpPost("staff/register")]
    [ProducesResponseType(typeof(RegisterStaffResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterStaff(
        [FromBody] RegisterStaffRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterStaffCommand(
                request.PropertyUid,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Role,
                User.GetRequiredSubject(),
                User.GetStaffUid(),
                User.GetPropertyUid(),
                User.IsPlatformAdmin(),
                User.IsHotelAdmin()),
            cancellationToken);

        return ToRegisterResult(result);
    }

    private async Task<IActionResult> LoginPropertyUser(
        StaffLoginRequest request,
        IReadOnlyList<string> requiredRoles,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new LoginStaffCommand(
                request.PropertyUid,
                request.Email,
                request.Password,
                requiredRoles),
            cancellationToken);

        return this.ToAuthResult(
            result.IsSuccessful,
            result.Data,
            result.ErrorCode,
            result.ErrorMessage);
    }

    private IActionResult ToRegisterResult(RegisterStaffResult result)
    {
        if (result.IsSuccessful)
            return StatusCode(StatusCodes.Status201Created, result.Data);

        return this.ToAuthResult(
            false,
            null,
            result.ErrorCode,
            result.ErrorMessage);
    }
}
