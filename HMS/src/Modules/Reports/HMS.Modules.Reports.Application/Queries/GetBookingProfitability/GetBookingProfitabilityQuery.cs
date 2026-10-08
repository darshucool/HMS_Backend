using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetBookingProfitability;

public sealed record GetBookingProfitabilityQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<IReadOnlyList<BookingProfitabilityDto>>>;

public sealed class GetBookingProfitabilityQueryHandler(
    IReportPropertyAccess propertyAccess,
    IPropertyReportRepository reports)
    : IRequestHandler<GetBookingProfitabilityQuery, ReportResult<IReadOnlyList<BookingProfitabilityDto>>>
{
    public async Task<ReportResult<IReadOnlyList<BookingProfitabilityDto>>> Handle(
        GetBookingProfitabilityQuery request,
        CancellationToken cancellationToken)
    {
        var access = await PropertyReportGuard.Authorize<IReadOnlyList<BookingProfitabilityDto>>(
            propertyAccess,
            request.PropertyUid,
            request.ActorSubject,
            request.IsPlatformAdmin,
            cancellationToken);
        if (access is not null)
            return access;

        var items = await reports.GetBookingProfitabilityAsync(request.PropertyUid, cancellationToken);
        return ReportResult<IReadOnlyList<BookingProfitabilityDto>>.Success(items);
    }
}
