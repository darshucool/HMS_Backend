namespace HMS.Modules.Reports.Application.Common;

public enum ReportResultStatus
{
    Success,
    Validation,
    NotFound,
    Forbidden
}

public sealed record ReportResult<T>(
    ReportResultStatus Status,
    T? Value = default,
    string? Error = null)
{
    public bool IsSuccess => Status == ReportResultStatus.Success;

    public static ReportResult<T> Success(T value) => new(ReportResultStatus.Success, value);
    public static ReportResult<T> Validation(string error) => new(ReportResultStatus.Validation, default, error);
    public static ReportResult<T> NotFound(string error) => new(ReportResultStatus.NotFound, default, error);
    public static ReportResult<T> Forbidden(string error) => new(ReportResultStatus.Forbidden, default, error);
}
