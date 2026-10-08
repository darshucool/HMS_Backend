namespace HMS.Modules.Identity.Application.Commands.LoginStaff;

public sealed record LoginStaffResult
{
    public bool IsSuccessful { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public StaffLoginResponse? Data { get; init; }

    public static LoginStaffResult Success(StaffLoginResponse data)
    {
        return new LoginStaffResult
        {
            IsSuccessful = true,
            Data = data
        };
    }

    public static LoginStaffResult Failure(
        string errorCode,
        string errorMessage)
    {
        return new LoginStaffResult
        {
            IsSuccessful = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }
}

public sealed record StaffLoginResponse(
    Guid StaffUid,
    Guid PropertyUid,
    IReadOnlyList<Guid> PropertyUids,
    string Email,
    string FullName,
    string[] Roles,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
