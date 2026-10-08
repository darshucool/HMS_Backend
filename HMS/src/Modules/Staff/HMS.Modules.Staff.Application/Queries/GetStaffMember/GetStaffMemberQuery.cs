using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using MediatR;

namespace HMS.Modules.Staff.Application.Queries.GetStaffMember;

public sealed record GetStaffMemberQuery(
    Guid StaffUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<StaffMemberDto>>;

public sealed class GetStaffMemberQueryHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<GetStaffMemberQuery, StaffResult<StaffMemberDto>>
{
    public async Task<StaffResult<StaffMemberDto>> Handle(
        GetStaffMemberQuery request,
        CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetContextAsync(request.StaffUid, cancellationToken);
        if (staff is null || staff.IsArchived)
            return StaffResult<StaffMemberDto>.NotFound("Staff member was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, staff.PropertyUid, false, cancellationToken))
        {
            return StaffResult<StaffMemberDto>.Forbidden("You cannot view this staff member.");
        }

        var detail = await staffRepository.GetDetailAsync(request.StaffUid, cancellationToken);
        return detail is null
            ? StaffResult<StaffMemberDto>.NotFound("Staff member was not found.")
            : StaffResult<StaffMemberDto>.Success(detail);
    }
}
