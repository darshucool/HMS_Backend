using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using MediatR;

namespace HMS.Modules.Staff.Application.Commands.DeleteStaffRole;

public sealed record DeleteStaffRoleCommand(
    Guid PropertyUid,
    Guid StaffRoleUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<bool>>;

public sealed class DeleteStaffRoleCommandHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRoleRepository staffRoleRepository)
    : IRequestHandler<DeleteStaffRoleCommand, StaffResult<bool>>
{
    public async Task<StaffResult<bool>> Handle(
        DeleteStaffRoleCommand request,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetPropertyAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return StaffResult<bool>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, true, cancellationToken))
        {
            return StaffResult<bool>.Forbidden("You cannot delete staff roles for this property.");
        }

        var status = await staffRoleRepository.ArchiveAsync(
            request.StaffRoleUid,
            property.OrganizationId,
            request.ActorSubject,
            cancellationToken);

        return status switch
        {
            StaffRoleDeleteStatus.Deleted => StaffResult<bool>.Success(true),
            StaffRoleDeleteStatus.InUse => StaffResult<bool>.Conflict("Staff members still use this role."),
            _ => StaffResult<bool>.NotFound("Staff role was not found.")
        };
    }
}
