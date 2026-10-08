using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using MediatR;

namespace HMS.Modules.Staff.Application.Commands.CreateStaffRole;

public sealed record CreateStaffRoleCommand(
    Guid PropertyUid,
    string Name,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<StaffRoleDto>>;

public sealed class CreateStaffRoleCommandHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRoleRepository staffRoleRepository)
    : IRequestHandler<CreateStaffRoleCommand, StaffResult<StaffRoleDto>>
{
    public async Task<StaffResult<StaffRoleDto>> Handle(
        CreateStaffRoleCommand request,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetPropertyAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return StaffResult<StaffRoleDto>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, true, cancellationToken))
        {
            return StaffResult<StaffRoleDto>.Forbidden("You cannot create staff roles for this property.");
        }

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
            return StaffResult<StaffRoleDto>.Validation("Role name is required and cannot exceed 100 characters.");

        var name = request.Name.Trim();
        var code = ToCode(name);
        if (code.Length == 0)
            return StaffResult<StaffRoleDto>.Validation("Role name must contain letters or numbers.");

        if (await staffRoleRepository.CodeExistsAsync(property.OrganizationId, code, cancellationToken))
            return StaffResult<StaffRoleDto>.Conflict("A staff role with this name already exists.");

        var role = await staffRoleRepository.InsertAsync(
            property.OrganizationId,
            code,
            name,
            request.ActorSubject,
            cancellationToken);

        return StaffResult<StaffRoleDto>.Success(role);
    }

    private static string ToCode(string name)
    {
        var code = new string(name.ToUpperInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '_')
            .ToArray());
        while (code.Contains("__", StringComparison.Ordinal))
            code = code.Replace("__", "_", StringComparison.Ordinal);

        code = code.Trim('_');
        return code.Length <= 30 ? code : code[..30].TrimEnd('_');
    }
}
