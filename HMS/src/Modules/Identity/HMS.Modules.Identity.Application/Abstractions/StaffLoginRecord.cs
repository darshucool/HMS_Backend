using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HMS.Modules.Identity.Application.Abstractions
{
    public sealed class StaffLoginRecord
    {
        public Guid StaffUid { get; init; }

        public Guid PropertyUid { get; init; }

        public string Username { get; init; } = string.Empty;

        public string? Email { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string? LastName { get; init; }

        public string PasswordHash { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public bool IsLocked { get; init; }

        public int FailedLoginCount { get; init; }

        public DateTimeOffset? LockedUntilUtc { get; init; }

        public string[] Roles { get; init; } = [];

        public string FullName =>
            string.Join(
                " ",
                new[] { FirstName, LastName }
                    .Where(value => !string.IsNullOrWhiteSpace(value)));
    }
}
