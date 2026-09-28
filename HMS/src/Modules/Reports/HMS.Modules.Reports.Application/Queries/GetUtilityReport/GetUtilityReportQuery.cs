using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetUtilityReport;

public sealed record GetUtilityReportQuery(
    Guid PropertyUid,
    int Year,
    int Month,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<IReadOnlyList<UtilityReportDto>>>;

public sealed class GetUtilityReportQueryHandler(
    IReportPropertyAccess propertyAccess,
    IPropertyReportRepository reports)
    : IRequestHandler<GetUtilityReportQuery, ReportResult<IReadOnlyList<UtilityReportDto>>>
{
    public async Task<ReportResult<IReadOnlyList<UtilityReportDto>>> Handle(
        GetUtilityReportQuery request,
        CancellationToken cancellationToken)
    {
        var period = ReportPeriod.Validate<IReadOnlyList<UtilityReportDto>>(request.Year, request.Month, out var reportMonth);
        if (period is not null)
            return period;

        var access = await PropertyReportGuard.Authorize<IReadOnlyList<UtilityReportDto>>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, cancellationToken);
        if (access is not null)
            return access;

        var items = await reports.GetUtilitiesAsync(request.PropertyUid, reportMonth, cancellationToken);
        return ReportResult<IReadOnlyList<UtilityReportDto>>.Success(items);
    }
}
