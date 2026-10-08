using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetMonthlyPropertySummary;

public sealed record GetMonthlyPropertySummaryQuery(
    Guid PropertyUid,
    int Year,
    int Month,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<MonthlyPropertySummaryDto>>;

public sealed class GetMonthlyPropertySummaryQueryHandler(
    IReportPropertyAccess propertyAccess,
    IMonthlyPropertySummaryRepository summaryRepository)
    : IRequestHandler<GetMonthlyPropertySummaryQuery, ReportResult<MonthlyPropertySummaryDto>>
{
    public async Task<ReportResult<MonthlyPropertySummaryDto>> Handle(
        GetMonthlyPropertySummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Month is < 1 or > 12)
            return ReportResult<MonthlyPropertySummaryDto>.Validation("Month must be between 1 and 12.");

        if (request.Year is < 2000 or > 2100)
            return ReportResult<MonthlyPropertySummaryDto>.Validation("Year must be between 2000 and 2100.");

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return ReportResult<MonthlyPropertySummaryDto>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, cancellationToken))
        {
            return ReportResult<MonthlyPropertySummaryDto>.Forbidden(
                "You cannot view reports for this property.");
        }

        var reportMonth = new DateOnly(request.Year, request.Month, 1);
        var summary = await summaryRepository.GetAsync(request.PropertyUid, reportMonth, cancellationToken);
        return ReportResult<MonthlyPropertySummaryDto>.Success(summary ?? Zero(property, reportMonth));
    }

    private static MonthlyPropertySummaryDto Zero(ReportPropertyContext property, DateOnly reportMonth) => new(
        property.Uid,
        property.Name,
        reportMonth,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0);
}
