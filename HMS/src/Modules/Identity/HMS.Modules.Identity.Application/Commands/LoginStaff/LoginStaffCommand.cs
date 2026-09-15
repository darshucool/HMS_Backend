using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Identity.Application.Commands.LoginStaff
{
    public sealed record LoginStaffCommand(
    Guid PropertyUid,
    string Username,
    string Password
) : IRequest<LoginStaffResult>;
}
