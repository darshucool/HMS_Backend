using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;
using HMS.Modules.Reports.Application.DTOs;
using MediatR;

namespace HMS.Modules.Reports.Application.Queries.GetExpenseReport;

public sealed record GetExpenseReportQuery(
    Guid PropertyUid,
    int Year,
    int Month,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<ReportResult<ExpenseReportDto>>;

public sealed class GetExpenseReportQueryHandler(
    IReportPropertyAccess propertyAccess,
    IPropertyReportRepository reports)
    : IRequestHandler<GetExpenseReportQuery, ReportResult<ExpenseReportDto>>
{
    public async Task<ReportResult<ExpenseReportDto>> Handle(
        GetExpenseReportQuery request,
        CancellationToken cancellationToken)
    {
        var period = ReportPeriod.Validate<ExpenseReportDto>(request.Year, request.Month, out var reportMonth);
        if (period is not null)
            return period;

        var access = await PropertyReportGuard.Authorize<ExpenseReportDto>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, cancellationToken);
        if (access is not null)
            return access;

        var report = await reports.GetExpensesAsync(request.PropertyUid, reportMonth, cancellationToken);
        return ReportResult<ExpenseReportDto>.Success(report ?? new ExpenseReportDto(reportMonth, 0, 0, 0, 0));
    }
}
