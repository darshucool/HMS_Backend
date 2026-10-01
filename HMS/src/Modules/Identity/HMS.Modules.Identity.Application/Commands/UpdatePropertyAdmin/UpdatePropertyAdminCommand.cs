using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.DTOs;
using MediatR;

namespace HMS.Modules.Identity.Application.Commands.UpdatePropertyAdmin;

public sealed record UpdatePropertyAdminCommand(
    Guid PropertyUid,
    Guid AdminId,
    string FirstName,
    string? LastName,
    bool IsActive) : IRequest<PropertyAdminResult<PropertyAdminDto>>;

internal sealed class UpdatePropertyAdminCommandHandler(IStaffRepository staffRepository)
    : IRequestHandler<UpdatePropertyAdminCommand, PropertyAdminResult<PropertyAdminDto>>
{
    public async Task<PropertyAdminResult<PropertyAdminDto>> Handle(
        UpdatePropertyAdminCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Trim().Length > 100)
        {
            return PropertyAdminResult<PropertyAdminDto>.Failure(
                "validation_error",
                "First name is required and cannot exceed 100 characters.");
        }

        if (request.LastName is { Length: > 100 })
        {
            return PropertyAdminResult<PropertyAdminDto>.Failure(
                "validation_error",
                "Last name cannot exceed 100 characters.");
        }

        var property = await staffRepository.GetPropertyByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return PropertyAdminResult<PropertyAdminDto>.Failure("not_found", "Property was not found.");

        var updated = await staffRepository.UpdatePropertyAdminAsync(
            request.PropertyUid,
            request.AdminId,
            request.FirstName.Trim(),
            string.IsNullOrWhiteSpace(request.LastName) ? null : request.LastName.Trim(),
            request.IsActive,
            cancellationToken);

        if (!updated)
            return PropertyAdminResult<PropertyAdminDto>.Failure("not_found", "Admin was not found for this property.");

        var admin = await staffRepository.GetPropertyAdminAsync(
            request.PropertyUid,
            request.AdminId,
            cancellationToken);

        return admin is null
            ? PropertyAdminResult<PropertyAdminDto>.Failure("not_found", "Admin was not found for this property.")
            : PropertyAdminResult<PropertyAdminDto>.Success(new PropertyAdminDto(
                admin.AdminId,
                admin.PropertyUid,
                admin.Email,
                admin.FirstName,
                admin.LastName,
                admin.IsActive,
                admin.AssignedAt));
    }
}
