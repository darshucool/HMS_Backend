using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using HMS.Modules.Staff.Domain.Enums;
using MediatR;

namespace HMS.Modules.Staff.Application.Commands.CreateStaffMember;

public sealed record CreateStaffMemberCommand(
    Guid PropertyUid,
    Guid StaffRoleUid,
    string EmployeeNumber,
    string FirstName,
    string? LastName,
    string? Phone,
    string? Email,
    EmploymentType EmploymentType,
    decimal? BasicSalary,
    decimal? DailyRate,
    decimal? HourlyRate,
    DateOnly? JoinedDate,
    DateOnly? LeftDate,
    StaffMemberStatus Status,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<StaffMemberDto>>;

public sealed class CreateStaffMemberCommandHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<CreateStaffMemberCommand, StaffResult<StaffMemberDto>>
{
    public async Task<StaffResult<StaffMemberDto>> Handle(
        CreateStaffMemberCommand request,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetPropertyAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return StaffResult<StaffMemberDto>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, true, cancellationToken))
        {
            return StaffResult<StaffMemberDto>.Forbidden("You cannot add staff to this property.");
        }

        var error = StaffProfileRules.Validate(
            request.EmployeeNumber,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Email,
            request.BasicSalary,
            request.DailyRate,
            request.HourlyRate,
            request.JoinedDate,
            request.LeftDate);
        if (error is not null)
            return StaffResult<StaffMemberDto>.Validation(error);

        var role = await staffRepository.GetRoleAsync(request.StaffRoleUid, cancellationToken);
        if (role is null || role.OrganizationId != property.OrganizationId)
            return StaffResult<StaffMemberDto>.NotFound("Staff role was not found for this organization.");

        var employeeNumber = request.EmployeeNumber.Trim();
        if (await staffRepository.EmployeeNumberExistsAsync(property.Id, employeeNumber, null, cancellationToken))
            return StaffResult<StaffMemberDto>.Conflict("A staff member with this employee number already exists.");

        var uid = await staffRepository.InsertAsync(
            property.OrganizationId,
            property.Id,
            role.Id,
            employeeNumber,
            request.FirstName.Trim(),
            StaffProfileRules.TrimOrNull(request.LastName),
            StaffProfileRules.TrimOrNull(request.Phone),
            StaffProfileRules.TrimOrNull(request.Email),
            request.EmploymentType.ToDatabaseValue(),
            request.BasicSalary,
            request.DailyRate,
            request.HourlyRate,
            request.JoinedDate,
            request.LeftDate,
            request.Status.ToDatabaseValue(),
            request.ActorSubject,
            cancellationToken);

        var detail = await staffRepository.GetDetailAsync(uid, cancellationToken);
        return detail is null
            ? StaffResult<StaffMemberDto>.NotFound("Staff member was not found.")
            : StaffResult<StaffMemberDto>.Success(detail);
    }
}
