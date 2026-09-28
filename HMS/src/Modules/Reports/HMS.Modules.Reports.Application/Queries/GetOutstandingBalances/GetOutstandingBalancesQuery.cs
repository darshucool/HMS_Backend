using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetOutstandingBalances;

public sealed record GetOutstandingBalancesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<IReadOnlyList<OutstandingBalanceDto>>>;

public sealed class GetOutstandingBalancesQueryHandler(
    IReportPropertyAccess propertyAccess,
    IPropertyReportRepository reports)
    : IRequestHandler<GetOutstandingBalancesQuery, ReportResult<IReadOnlyList<OutstandingBalanceDto>>>
{
    public async Task<ReportResult<IReadOnlyList<OutstandingBalanceDto>>> Handle(
        GetOutstandingBalancesQuery request,
        CancellationToken cancellationToken)
    {
        var access = await PropertyReportGuard.Authorize<IReadOnlyList<OutstandingBalanceDto>>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, cancellationToken);
        if (access is not null)
            return access;

        var items = await reports.GetOutstandingBalancesAsync(request.PropertyUid, cancellationToken);
        return ReportResult<IReadOnlyList<OutstandingBalanceDto>>.Success(items);
    }
}
