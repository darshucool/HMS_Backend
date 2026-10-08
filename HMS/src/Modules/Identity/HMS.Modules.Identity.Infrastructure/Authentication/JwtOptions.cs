using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Identity.Infrastructure.Authentication
{
    internal sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public string SecretKey { get; init; } = string.Empty;

        public int AccessTokenMinutes { get; init; } = 30;

        public int RefreshTokenDays { get; init; } = 14;
    }
}
