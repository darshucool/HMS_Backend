using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using MediatR;

namespace HMS.Modules.Staff.Application.Queries.GetStaffRoles;

public sealed record GetStaffRolesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<IReadOnlyList<StaffRoleDto>>>;

public sealed class GetStaffRolesQueryHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRoleRepository staffRoleRepository)
    : IRequestHandler<GetStaffRolesQuery, StaffResult<IReadOnlyList<StaffRoleDto>>>
{
    public async Task<StaffResult<IReadOnlyList<StaffRoleDto>>> Handle(
        GetStaffRolesQuery request,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetPropertyAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return StaffResult<IReadOnlyList<StaffRoleDto>>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, false, cancellationToken))
        {
            return StaffResult<IReadOnlyList<StaffRoleDto>>.Forbidden(
                "You cannot view staff roles for this property.");
        }

        var items = await staffRoleRepository.ListAsync(property.OrganizationId, cancellationToken);
        return StaffResult<IReadOnlyList<StaffRoleDto>>.Success(items);
    }
}
