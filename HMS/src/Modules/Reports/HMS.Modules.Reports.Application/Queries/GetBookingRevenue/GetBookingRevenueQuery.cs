using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetBookingRevenue;

public sealed record GetBookingRevenueQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<IReadOnlyList<BookingRevenueDto>>>;

public sealed class GetBookingRevenueQueryHandler(
    IReportPropertyAccess propertyAccess,
    IBookingRevenueRepository revenueRepository)
    : IRequestHandler<GetBookingRevenueQuery, ReportResult<IReadOnlyList<BookingRevenueDto>>>
{
    public async Task<ReportResult<IReadOnlyList<BookingRevenueDto>>> Handle(
        GetBookingRevenueQuery request,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return ReportResult<IReadOnlyList<BookingRevenueDto>>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, cancellationToken))
        {
            return ReportResult<IReadOnlyList<BookingRevenueDto>>.Forbidden(
                "You cannot view reports for this property.");
        }

        var items = await revenueRepository.GetByPropertyUidAsync(request.PropertyUid, cancellationToken);
        return ReportResult<IReadOnlyList<BookingRevenueDto>>.Success(items);
    }
}
