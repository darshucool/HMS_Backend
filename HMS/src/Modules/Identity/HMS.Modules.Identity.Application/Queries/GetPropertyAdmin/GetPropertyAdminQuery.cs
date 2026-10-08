using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.DTOs;
using MediatR;

namespace HMS.Modules.Identity.Application.Queries.GetPropertyAdmin;

public sealed record GetPropertyAdminQuery(Guid PropertyUid, Guid AdminId)
    : IRequest<PropertyAdminResult<PropertyAdminDto>>;

internal sealed class GetPropertyAdminQueryHandler(IStaffRepository staffRepository)
    : IRequestHandler<GetPropertyAdminQuery, PropertyAdminResult<PropertyAdminDto>>
{
    public async Task<PropertyAdminResult<PropertyAdminDto>> Handle(
        GetPropertyAdminQuery request,
        CancellationToken cancellationToken)
    {
        var property = await staffRepository.GetPropertyByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return PropertyAdminResult<PropertyAdminDto>.Failure("not_found", "Property was not found.");

        var admin = await staffRepository.GetPropertyAdminAsync(
            request.PropertyUid,
            request.AdminId,
            cancellationToken);

        return admin is null
            ? PropertyAdminResult<PropertyAdminDto>.Failure("not_found", "Admin was not found for this property.")
            : PropertyAdminResult<PropertyAdminDto>.Success(ToDto(admin));
    }

    private static PropertyAdminDto ToDto(PropertyAdminRecord admin) => new(
        admin.AdminId,
        admin.PropertyUid,
        admin.Email,
        admin.FirstName,
        admin.LastName,
        admin.IsActive,
        admin.AssignedAt);
}
