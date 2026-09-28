using HMS.Modules.Reports.Application.Common;

namespace HMS.Modules.Reports.Application.Common;

public static class ReportPeriod
{
    public static ReportResult<T>? Validate<T>(int year, int month, out DateOnly reportMonth)
    {
        reportMonth = default;
        if (month is < 1 or > 12)
            return ReportResult<T>.Validation("Month must be between 1 and 12.");

        if (year is < 2000 or > 2100)
            return ReportResult<T>.Validation("Year must be between 2000 and 2100.");

        reportMonth = new DateOnly(year, month, 1);
        return null;
    }
}
