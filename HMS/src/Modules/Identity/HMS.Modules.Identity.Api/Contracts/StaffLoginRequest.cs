using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Identity.Api.Contracts
{
    public sealed record StaffLoginRequest(
     Guid PropertyUid,
     string Username,
     string Password);
}
