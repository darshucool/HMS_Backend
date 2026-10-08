using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetGuestProfitability;

public sealed record GetGuestProfitabilityQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<IReadOnlyList<GuestProfitabilityDto>>>;

public sealed class GetGuestProfitabilityQueryHandler(
    IReportPropertyAccess propertyAccess,
    IPropertyReportRepository reports)
    : IRequestHandler<GetGuestProfitabilityQuery, ReportResult<IReadOnlyList<GuestProfitabilityDto>>>
{
    public async Task<ReportResult<IReadOnlyList<GuestProfitabilityDto>>> Handle(
        GetGuestProfitabilityQuery request,
        CancellationToken cancellationToken)
    {
        var access = await PropertyReportGuard.Authorize<IReadOnlyList<GuestProfitabilityDto>>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, cancellationToken);
        if (access is not null)
            return access;

        var items = await reports.GetGuestProfitabilityAsync(request.PropertyUid, cancellationToken);
        return ReportResult<IReadOnlyList<GuestProfitabilityDto>>.Success(items);
    }
}
