namespace HMS.Modules.Guests.Application.Common;

public enum GuestResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}

public sealed record GuestResult<T>(
    GuestResultStatus Status,
    T? Value = default,
    string? Error = null)
{
    public bool IsSuccess => Status == GuestResultStatus.Success;

    public static GuestResult<T> Success(T value) => new(GuestResultStatus.Success, value);
    public static GuestResult<T> Validation(string error) => new(GuestResultStatus.Validation, default, error);
    public static GuestResult<T> NotFound(string error) => new(GuestResultStatus.NotFound, default, error);
    public static GuestResult<T> Conflict(string error) => new(GuestResultStatus.Conflict, default, error);
    public static GuestResult<T> Forbidden(string error) => new(GuestResultStatus.Forbidden, default, error);
}
