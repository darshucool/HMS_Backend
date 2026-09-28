namespace HMS.Modules.Payments.Application.Common;

public enum PaymentResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}

public sealed record PaymentResult<T>(
    PaymentResultStatus Status,
    T? Value = default,
    string? Error = null)
{
    public bool IsSuccess => Status == PaymentResultStatus.Success;

    public static PaymentResult<T> Success(T value) => new(PaymentResultStatus.Success, value);
    public static PaymentResult<T> Validation(string error) => new(PaymentResultStatus.Validation, default, error);
    public static PaymentResult<T> NotFound(string error) => new(PaymentResultStatus.NotFound, default, error);
    public static PaymentResult<T> Conflict(string error) => new(PaymentResultStatus.Conflict, default, error);
    public static PaymentResult<T> Forbidden(string error) => new(PaymentResultStatus.Forbidden, default, error);
}
