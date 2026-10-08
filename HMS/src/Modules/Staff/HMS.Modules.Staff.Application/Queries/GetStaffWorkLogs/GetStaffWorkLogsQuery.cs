using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using MediatR;

namespace HMS.Modules.Staff.Application.Queries.GetStaffWorkLogs;

public sealed record GetStaffWorkLogsQuery(
    Guid StaffUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<IReadOnlyList<StaffWorkLogDto>>>;

public sealed class GetStaffWorkLogsQueryHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<GetStaffWorkLogsQuery, StaffResult<IReadOnlyList<StaffWorkLogDto>>>
{
    public async Task<StaffResult<IReadOnlyList<StaffWorkLogDto>>> Handle(
        GetStaffWorkLogsQuery request,
        CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetContextAsync(request.StaffUid, cancellationToken);
        if (staff is null || staff.IsArchived)
            return StaffResult<IReadOnlyList<StaffWorkLogDto>>.NotFound("Staff member was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, staff.PropertyUid, false, cancellationToken))
        {
            return StaffResult<IReadOnlyList<StaffWorkLogDto>>.Forbidden(
                "You cannot view work logs for this staff member.");
        }

        var items = await staffRepository.GetWorkLogsAsync(staff.Id, cancellationToken);
        return StaffResult<IReadOnlyList<StaffWorkLogDto>>.Success(items);
    }
}
