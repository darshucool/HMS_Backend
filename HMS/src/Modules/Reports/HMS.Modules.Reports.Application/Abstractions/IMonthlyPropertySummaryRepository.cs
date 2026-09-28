using HMS.Modules.Reports.Application.DTOs;

namespace HMS.Modules.Reports.Application.Abstractions;

public sealed record ReportPropertyContext(Guid Uid, string Name, bool IsArchived);

public interface IReportPropertyAccess
{
    Task<ReportPropertyContext?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken);
    Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        CancellationToken cancellationToken);
}

public interface IMonthlyPropertySummaryRepository
{
    Task<MonthlyPropertySummaryDto?> GetAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken);
}
