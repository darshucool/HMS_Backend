using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.Authentication;
using HMS.Modules.Identity.Application.Commands.LoginStaff;
using MediatR;

namespace HMS.Modules.Identity.Application.Commands.LoginSuperAdmin;

public sealed record LoginSuperAdminCommand(
    string Username,
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
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(LoginStaffResult.Failure(
                "validation_error",
                "Username and password are required."));
        }

        if (!SuperAdminCredentials.Matches(request.Username, request.Password))
        {
            return Task.FromResult(LoginStaffResult.Failure(
                "invalid_credentials",
                "Invalid username or password."));
        }

        var currentTime = timeProvider.GetUtcNow();
        var staff = new StaffLoginRecord
        {
            StaffUid = SuperAdminCredentials.StaffUid,
            PropertyUid = Guid.Empty,
            Username = SuperAdminCredentials.Username,
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
                staff.Username,
                staff.FullName,
                staff.Email,
                staff.Roles,
                token.AccessToken,
                token.ExpiresAtUtc)));
    }
}
