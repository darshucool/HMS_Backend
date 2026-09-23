using HMS.Modules.Identity.Application.Commands.LoginStaff;
using MediatR;

namespace HMS.Modules.Identity.Application.Commands.RegisterStaff;

public sealed record RegisterStaffCommand(
    Guid PropertyUid,
    string Username,
    string Password,
    string FirstName,
    string? LastName,
    string? Email,
    string RoleCode,
    string ActorSubject,
    Guid? ActorStaffUid,
    Guid? ActorPropertyUid,
    bool IsPlatformAdmin,
    bool IsHotelAdmin) : IRequest<RegisterStaffResult>;

public sealed record RegisterStaffResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public RegisterStaffResponse? Data { get; init; }

    public static RegisterStaffResult Success(RegisterStaffResponse data) => new()
    {
        IsSuccessful = true,
        Data = data
    };

    public static RegisterStaffResult Failure(string errorCode, string errorMessage) => new()
    {
        IsSuccessful = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };
}

public sealed record RegisterStaffResponse(
    Guid StaffUid,
    Guid PropertyUid,
    string Username,
    string FullName,
    string? Email,
    string Role);
