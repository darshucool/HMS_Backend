using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetPaymentSummary;

public sealed record GetPaymentSummaryQuery(
    Guid PropertyUid,
    int Year,
    int Month,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<IReadOnlyList<PaymentMethodSummaryDto>>>;

public sealed class GetPaymentSummaryQueryHandler(
    IReportPropertyAccess propertyAccess,
    IPropertyReportRepository reports)
    : IRequestHandler<GetPaymentSummaryQuery, ReportResult<IReadOnlyList<PaymentMethodSummaryDto>>>
{
    public async Task<ReportResult<IReadOnlyList<PaymentMethodSummaryDto>>> Handle(
        GetPaymentSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var period = ReportPeriod.Validate<IReadOnlyList<PaymentMethodSummaryDto>>(request.Year, request.Month, out var reportMonth);
        if (period is not null)
            return period;

        var access = await PropertyReportGuard.Authorize<IReadOnlyList<PaymentMethodSummaryDto>>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, cancellationToken);
        if (access is not null)
            return access;

        var items = await reports.GetPaymentSummaryAsync(request.PropertyUid, reportMonth, cancellationToken);
        return ReportResult<IReadOnlyList<PaymentMethodSummaryDto>>.Success(items);
    }
}
