using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetOccupancy;

public sealed record GetOccupancyQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<IReadOnlyList<OccupancyStayDto>>>;

public sealed class GetOccupancyQueryHandler(
    IReportPropertyAccess propertyAccess,
    IPropertyReportRepository reports)
    : IRequestHandler<GetOccupancyQuery, ReportResult<IReadOnlyList<OccupancyStayDto>>>
{
    public async Task<ReportResult<IReadOnlyList<OccupancyStayDto>>> Handle(
        GetOccupancyQuery request,
        CancellationToken cancellationToken)
    {
        var access = await PropertyReportGuard.Authorize<IReadOnlyList<OccupancyStayDto>>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, cancellationToken);
        if (access is not null)
            return access;

        var items = await reports.GetOccupancyAsync(request.PropertyUid, cancellationToken);
        return ReportResult<IReadOnlyList<OccupancyStayDto>>.Success(items);
    }
}
