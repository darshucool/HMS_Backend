using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.Authentication;
using HMS.Modules.Identity.Application.Commands.LoginStaff;
using MediatR;

namespace HMS.Modules.Identity.Application.Commands.RefreshSession;

public sealed record RefreshSessionCommand(string RefreshToken) : IRequest<LoginStaffResult>;

internal sealed class RefreshSessionCommandHandler(
    IRefreshTokenStore refreshTokenStore,
    IStaffRepository staffRepository,
    IStaffTokenService tokenService,
    TimeProvider timeProvider)
    : IRequestHandler<RefreshSessionCommand, LoginStaffResult>
{
    public async Task<LoginStaffResult> Handle(
        RefreshSessionCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return LoginStaffResult.Failure(
                "validation_error",
                "Refresh token is required.");
        }

        var currentTime = timeProvider.GetUtcNow();
        var stored = await refreshTokenStore.TakeValidAsync(
            request.RefreshToken.Trim(),
            currentTime,
            cancellationToken);

        if (stored is null)
        {
            return LoginStaffResult.Failure(
                "invalid_refresh_token",
                "The refresh token is invalid or expired. Sign in again.");
        }

        StaffLoginRecord? staff;
        if (stored.StaffUid == SuperAdminCredentials.StaffUid)
        {
            staff = new StaffLoginRecord
            {
                StaffUid = SuperAdminCredentials.StaffUid,
                PropertyUid = Guid.Empty,
                Username = SuperAdminCredentials.Email,
                Email = SuperAdminCredentials.Email,
                FirstName = SuperAdminCredentials.FirstName,
                LastName = SuperAdminCredentials.LastName,
                IsActive = true,
                Roles = [IdentityRoles.PlatformAdmin]
            };
        }
        else
        {
            staff = await staffRepository.GetByUidAsync(stored.StaffUid, cancellationToken);
            if (staff is null || !staff.IsActive)
            {
                return LoginStaffResult.Failure(
                    "account_inactive",
                    "This account can no longer refresh a session.");
            }
        }

        var accessToken = tokenService.Generate(staff, currentTime);
        var refreshToken = await refreshTokenStore.IssueAsync(
            staff.StaffUid,
            staff.PropertyUid == Guid.Empty ? null : staff.PropertyUid,
            currentTime,
            cancellationToken);

        return LoginStaffResult.Success(
            new StaffLoginResponse(
                staff.StaffUid,
                staff.PropertyUid,
                staff.PropertyUids,
                staff.Email ?? string.Empty,
                staff.FullName,
                staff.Roles,
                accessToken.AccessToken,
                accessToken.ExpiresAtUtc,
                refreshToken.RefreshToken,
                refreshToken.ExpiresAtUtc));
    }
}
