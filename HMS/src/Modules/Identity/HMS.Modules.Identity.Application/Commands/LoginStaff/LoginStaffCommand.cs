using MediatR;

namespace HMS.Modules.Identity.Application.Commands.LoginStaff;

public sealed record LoginStaffCommand(
    Guid? PropertyUid,
    string Email,
    string Password,
    IReadOnlyList<string>? RequiredRoles = null) : IRequest<LoginStaffResult>;
