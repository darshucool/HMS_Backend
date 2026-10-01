using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using MediatR;

namespace HMS.Modules.Staff.Application.Queries.GetStaffMembers;

public sealed record GetStaffMembersQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<IReadOnlyList<StaffMemberDto>>>;

public sealed class GetStaffMembersQueryHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<GetStaffMembersQuery, StaffResult<IReadOnlyList<StaffMemberDto>>>
{
    public async Task<StaffResult<IReadOnlyList<StaffMemberDto>>> Handle(
        GetStaffMembersQuery request,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetPropertyAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return StaffResult<IReadOnlyList<StaffMemberDto>>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, false, cancellationToken))
        {
            return StaffResult<IReadOnlyList<StaffMemberDto>>.Forbidden("You cannot view staff for this property.");
        }

        var items = await staffRepository.GetByPropertyAsync(property.Id, cancellationToken);
        return StaffResult<IReadOnlyList<StaffMemberDto>>.Success(items);
    }
}
