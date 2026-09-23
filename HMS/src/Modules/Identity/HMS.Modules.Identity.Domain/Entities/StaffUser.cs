using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Identity.Domain.Entities
{
    public sealed class StaffUser
    {
        private StaffUser()
        {
        }

        public Guid Uid { get; private set; }

        public string Username { get; private set; } = string.Empty;

        public string NormalizedUsername { get; private set; } = string.Empty;

        public string? Email { get; private set; }

        public string? NormalizedEmail { get; private set; }

        public string FirstName { get; private set; } = string.Empty;

        public string? LastName { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;

        public bool IsActive { get; private set; }

        public bool IsLocked { get; private set; }

        public int FailedLoginCount { get; private set; }

        public DateTimeOffset? LockedUntilUtc { get; private set; }

        public DateTimeOffset? LastLoginUtc { get; private set; }

        public string FullName =>
            string.Join(
                " ",
                new[] { FirstName, LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));

        public bool IsCurrentlyLocked(DateTimeOffset currentTime)
        {
            return IsLocked ||
                   (LockedUntilUtc.HasValue &&
                    LockedUntilUtc.Value > currentTime);
        }

        public static StaffUser Create(
            string username,
            string firstName,
            string? lastName,
            string? email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(firstName);

            var cleanedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            return new StaffUser
            {
                Uid = Guid.NewGuid(),
                Username = username.Trim(),
                NormalizedUsername = Normalize(username),
                FirstName = firstName.Trim(),
                LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
                Email = cleanedEmail,
                NormalizedEmail = cleanedEmail?.ToUpperInvariant(),
                IsActive = true
            };
        }

        public void SetPasswordHash(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToUpperInvariant();
        }
    }
}
