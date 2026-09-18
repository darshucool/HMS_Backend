namespace HMS.Modules.Hotels.Application.Common;

public enum HotelResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}

public sealed record HotelResult<T>(
    HotelResultStatus Status,
    T? Value = default,
    string? Error = null)
{
    public bool IsSuccess => Status == HotelResultStatus.Success;

    public static HotelResult<T> Success(T value) => new(HotelResultStatus.Success, value);
    public static HotelResult<T> Validation(string error) => new(HotelResultStatus.Validation, default, error);
    public static HotelResult<T> NotFound(string error) => new(HotelResultStatus.NotFound, default, error);
    public static HotelResult<T> Conflict(string error) => new(HotelResultStatus.Conflict, default, error);
    public static HotelResult<T> Forbidden(string error) => new(HotelResultStatus.Forbidden, default, error);
}

