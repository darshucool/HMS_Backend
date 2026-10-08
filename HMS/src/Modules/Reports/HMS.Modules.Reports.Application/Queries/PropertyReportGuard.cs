using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Application.Common;

namespace HMS.Modules.Reports.Application.Queries;

internal static class PropertyReportGuard
{
    public static async Task<ReportResult<T>?> Authorize<T>(
        IReportPropertyAccess propertyAccess,
        Guid propertyUid,
        string actorSubject,
        bool isPlatformAdmin,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetByUidAsync(propertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return ReportResult<T>.NotFound("Property was not found.");

        if (!isPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(actorSubject, propertyUid, cancellationToken))
        {
            return ReportResult<T>.Forbidden("You cannot view reports for this property.");
        }

        return null;
    }
}
