using HMS.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HMS.Modules.Identity.Infrastructure.Authentication
{
    internal sealed class StaffTokenService : IStaffTokenService
    {
        private readonly JwtOptions _options;

        public StaffTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public StaffTokenResult Generate(
            StaffLoginRecord staff,
            DateTimeOffset currentTime)
        {
            var expiresAt =
                currentTime.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, staff.StaffUid.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, staff.Username),
                new("staff_uid", staff.StaffUid.ToString()),
                new("full_name", staff.FullName)
            };

            if (staff.PropertyUid != Guid.Empty)
            {
                claims.Add(new Claim("property_uid", staff.PropertyUid.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(staff.Email))
            {
                claims.Add(
                    new Claim(
                        JwtRegisteredClaimNames.Email,
                        staff.Email));
            }

            foreach (var role in staff.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: currentTime.UtcDateTime,
                expires: expiresAt.UtcDateTime,
                signingCredentials: credentials);

            return new StaffTokenResult(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt);
        }
    }
}
