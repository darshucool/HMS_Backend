using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.DTOs;
using MediatR;

namespace HMS.Modules.Identity.Application.Queries.GetPropertyAdmins;

public sealed record GetPropertyAdminsQuery(Guid PropertyUid)
    : IRequest<PropertyAdminResult<IReadOnlyList<PropertyAdminDto>>>;

internal sealed class GetPropertyAdminsQueryHandler(IStaffRepository staffRepository)
    : IRequestHandler<GetPropertyAdminsQuery, PropertyAdminResult<IReadOnlyList<PropertyAdminDto>>>
{
    public async Task<PropertyAdminResult<IReadOnlyList<PropertyAdminDto>>> Handle(
        GetPropertyAdminsQuery request,
        CancellationToken cancellationToken)
    {
        var property = await staffRepository.GetPropertyByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return PropertyAdminResult<IReadOnlyList<PropertyAdminDto>>.Failure("not_found", "Property was not found.");

        var admins = await staffRepository.GetPropertyAdminsAsync(request.PropertyUid, cancellationToken);
        return PropertyAdminResult<IReadOnlyList<PropertyAdminDto>>.Success(admins.Select(ToDto).ToList());
    }

    internal static PropertyAdminDto ToDto(PropertyAdminRecord admin) => new(
        admin.AdminId,
        admin.PropertyUid,
        admin.Email,
        admin.FirstName,
        admin.LastName,
        admin.IsActive,
        admin.AssignedAt);
}
