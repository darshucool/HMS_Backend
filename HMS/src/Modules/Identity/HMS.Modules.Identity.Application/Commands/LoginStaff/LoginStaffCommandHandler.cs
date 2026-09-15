using HMS.Modules.Identity.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HMS.Modules.Identity.Application.Commands.LoginStaff
{
    internal sealed class LoginStaffCommandHandler
    : IRequestHandler<LoginStaffCommand, LoginStaffResult>
    {
        private const int MaximumFailedAttempts = 5;

        private static readonly TimeSpan LockDuration =
            TimeSpan.FromMinutes(15);

        private readonly IStaffRepository _staffRepository;
        private readonly IStaffTokenService _tokenService;
        private readonly IPasswordHasher<StaffLoginRecord> _passwordHasher;
        private readonly TimeProvider _timeProvider;

        public LoginStaffCommandHandler(
            IStaffRepository staffRepository,
            IStaffTokenService tokenService,
            IPasswordHasher<StaffLoginRecord> passwordHasher,
            TimeProvider timeProvider)
        {
            _staffRepository = staffRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _timeProvider = timeProvider;
        }

        public async Task<LoginStaffResult> Handle(
            LoginStaffCommand request,
            CancellationToken cancellationToken)
        {
            if (request.PropertyUid == Guid.Empty ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return LoginStaffResult.Failure(
                    "validation_error",
                    "Property, username and password are required.");
            }

            var normalizedUsername =
                request.Username.Trim().ToUpperInvariant();

            var staff = await _staffRepository.GetForLoginAsync(
                normalizedUsername,
                request.PropertyUid,
                cancellationToken);

            // Return the same message for nonexistent users and incorrect passwords.
            if (staff is null)
            {
                return InvalidCredentials();
            }

            var currentTime = _timeProvider.GetUtcNow();

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

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                staff,
                staff.PasswordHash,
                request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                await _staffRepository.RecordFailedLoginAsync(
                    staff.StaffUid,
                    MaximumFailedAttempts,
                    LockDuration,
                    cancellationToken);

                return InvalidCredentials();
            }

            await _staffRepository.RecordSuccessfulLoginAsync(
                staff.StaffUid,
                cancellationToken);

            var token = _tokenService.Generate(staff, currentTime);

            return LoginStaffResult.Success(
                new StaffLoginResponse(
                    staff.StaffUid,
                    staff.PropertyUid,
                    staff.Username,
                    staff.FullName,
                    staff.Email,
                    staff.Roles,
                    token.AccessToken,
                    token.ExpiresAtUtc));
        }

        private static LoginStaffResult InvalidCredentials()
        {
            return LoginStaffResult.Failure(
                "invalid_credentials",
                "Invalid username or password.");
        }
    }
}
