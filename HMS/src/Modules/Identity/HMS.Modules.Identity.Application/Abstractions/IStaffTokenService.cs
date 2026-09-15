using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Identity.Application.Abstractions
{
    public interface IStaffTokenService
    {
        StaffTokenResult Generate(
            StaffLoginRecord staff,
            DateTimeOffset currentTime);
    }

    public sealed record StaffTokenResult(
        string AccessToken,
        DateTimeOffset ExpiresAtUtc);
}
