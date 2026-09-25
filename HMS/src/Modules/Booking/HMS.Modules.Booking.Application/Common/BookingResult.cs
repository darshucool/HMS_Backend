namespace HMS.Modules.Booking.Application.Common;

public enum BookingResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}

public sealed record BookingResult<T>(
    BookingResultStatus Status,
    T? Value = default,
    string? Error = null)
{
    public bool IsSuccess => Status == BookingResultStatus.Success;

    public static BookingResult<T> Success(T value) => new(BookingResultStatus.Success, value);
    public static BookingResult<T> Validation(string error) => new(BookingResultStatus.Validation, default, error);
    public static BookingResult<T> NotFound(string error) => new(BookingResultStatus.NotFound, default, error);
    public static BookingResult<T> Conflict(string error) => new(BookingResultStatus.Conflict, default, error);
    public static BookingResult<T> Forbidden(string error) => new(BookingResultStatus.Forbidden, default, error);
}
