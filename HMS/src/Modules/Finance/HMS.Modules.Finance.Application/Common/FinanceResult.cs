namespace HMS.Modules.Finance.Application.Common;

public enum FinanceResultStatus
{
    Success,
    Validation,
    NotFound,
    Conflict,
    Forbidden
}

public sealed record FinanceResult<T>(
    FinanceResultStatus Status,
    T? Value = default,
    string? Error = null)
{
    public bool IsSuccess => Status == FinanceResultStatus.Success;

    public static FinanceResult<T> Success(T value) => new(FinanceResultStatus.Success, value);
    public static FinanceResult<T> Validation(string error) => new(FinanceResultStatus.Validation, default, error);
    public static FinanceResult<T> NotFound(string error) => new(FinanceResultStatus.NotFound, default, error);
    public static FinanceResult<T> Conflict(string error) => new(FinanceResultStatus.Conflict, default, error);
    public static FinanceResult<T> Forbidden(string error) => new(FinanceResultStatus.Forbidden, default, error);
}
