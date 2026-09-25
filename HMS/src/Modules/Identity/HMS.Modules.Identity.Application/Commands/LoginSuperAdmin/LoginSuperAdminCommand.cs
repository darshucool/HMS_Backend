using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.Authentication;
using HMS.Modules.Identity.Application.Commands.LoginStaff;
using MediatR;

namespace HMS.Modules.Identity.Application.Commands.LoginSuperAdmin;

public sealed record LoginSuperAdminCommand(
    string Email,
    string Password) : IRequest<LoginStaffResult>;

internal sealed class LoginSuperAdminCommandHandler(
    IStaffTokenService tokenService,
    TimeProvider timeProvider)
    : IRequestHandler<LoginSuperAdminCommand, LoginStaffResult>
{
    public Task<LoginStaffResult> Handle(
        LoginSuperAdminCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(LoginStaffResult.Failure(
                "validation_error",
                "Email and password are required."));
        }

        if (!SuperAdminCredentials.Matches(request.Email, request.Password))
        {
            return Task.FromResult(LoginStaffResult.Failure(
                "invalid_credentials",
                "Invalid email or password."));
        }

        var currentTime = timeProvider.GetUtcNow();
        var staff = new StaffLoginRecord
        {
            StaffUid = SuperAdminCredentials.StaffUid,
            PropertyUid = Guid.Empty,
            Username = SuperAdminCredentials.Email,
            Email = SuperAdminCredentials.Email,
            FirstName = SuperAdminCredentials.FirstName,
            LastName = SuperAdminCredentials.LastName,
            PasswordHash = string.Empty,
            IsActive = true,
            Roles = [IdentityRoles.PlatformAdmin]
        };

        var token = tokenService.Generate(staff, currentTime);

        return Task.FromResult(LoginStaffResult.Success(
            new StaffLoginResponse(
                staff.StaffUid,
                staff.PropertyUid,
                [],
                staff.Email ?? SuperAdminCredentials.Email,
                staff.FullName,
                staff.Roles,
                token.AccessToken,
                token.ExpiresAtUtc)));
    }
}
