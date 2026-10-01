namespace HMS.Modules.Staff.Application.Common;

public enum StaffResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}

public sealed record StaffResult<T>(
    StaffResultStatus Status,
    T? Value = default,
    string? Error = null)
{
    public bool IsSuccess => Status == StaffResultStatus.Success;

    public static StaffResult<T> Success(T value) => new(StaffResultStatus.Success, value);
    public static StaffResult<T> Validation(string error) => new(StaffResultStatus.Validation, default, error);
    public static StaffResult<T> NotFound(string error) => new(StaffResultStatus.NotFound, default, error);
    public static StaffResult<T> Conflict(string error) => new(StaffResultStatus.Conflict, default, error);
    public static StaffResult<T> Forbidden(string error) => new(StaffResultStatus.Forbidden, default, error);
}
