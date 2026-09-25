using HMS.Modules.Identity.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HMS.Modules.Identity.Application.Commands.LoginStaff;

internal sealed class LoginStaffCommandHandler(
    IStaffRepository staffRepository,
    IStaffTokenService tokenService,
    IPasswordHasher<StaffLoginRecord> passwordHasher,
    TimeProvider timeProvider)
    : IRequestHandler<LoginStaffCommand, LoginStaffResult>
{
    private const int MaximumFailedAttempts = 5;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    public async Task<LoginStaffResult> Handle(
        LoginStaffCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return LoginStaffResult.Failure(
                "validation_error",
                "Email and password are required.");
        }

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();

        var staff = await staffRepository.GetForLoginAsync(
            normalizedEmail,
            request.PropertyUid is Guid propertyUid && propertyUid != Guid.Empty
                ? propertyUid
                : null,
            cancellationToken);

        if (staff is null)
            return InvalidCredentials();

        var currentTime = timeProvider.GetUtcNow();

        if (!staff.IsActive)
        {
            return LoginStaffResult.Failure(
                "account_inactive",
                "This staff account is inactive.");
        }

        if (staff.IsLocked ||
            (staff.LockedUntilUtc.HasValue &&
             staff.LockedUntilUtc.Value > currentTime))
        {
            return LoginStaffResult.Failure(
                "account_locked",
                "This account is temporarily locked. Please try again later.");
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            staff,
            staff.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            await staffRepository.RecordFailedLoginAsync(
                staff.StaffUid,
                MaximumFailedAttempts,
                LockDuration,
                cancellationToken);

            return InvalidCredentials();
        }

        if (request.RequiredRoles is { Count: > 0 } &&
            !staff.Roles.Any(role =>
                request.RequiredRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            return LoginStaffResult.Failure(
                "invalid_credentials",
                "This account cannot sign in with this login.");
        }

        await staffRepository.RecordSuccessfulLoginAsync(
            staff.StaffUid,
            cancellationToken);

        var token = tokenService.Generate(staff, currentTime);

        return LoginStaffResult.Success(
            new StaffLoginResponse(
                staff.StaffUid,
                staff.PropertyUid,
                staff.PropertyUids,
                staff.Email ?? string.Empty,
                staff.FullName,
                staff.Roles,
                token.AccessToken,
                token.ExpiresAtUtc));
    }

    private static LoginStaffResult InvalidCredentials() =>
        LoginStaffResult.Failure(
            "invalid_credentials",
            "Invalid email or password.");
}
