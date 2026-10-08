using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using HMS.Modules.Staff.Domain.Enums;
using MediatR;

namespace HMS.Modules.Staff.Application.Commands.UpdateStaffMember;

public sealed record UpdateStaffMemberCommand(
    Guid StaffUid,
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

public sealed class UpdateStaffMemberCommandHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<UpdateStaffMemberCommand, StaffResult<StaffMemberDto>>
{
    public async Task<StaffResult<StaffMemberDto>> Handle(
        UpdateStaffMemberCommand request,
        CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetContextAsync(request.StaffUid, cancellationToken);
        if (staff is null || staff.IsArchived)
            return StaffResult<StaffMemberDto>.NotFound("Staff member was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, staff.PropertyUid, true, cancellationToken))
        {
            return StaffResult<StaffMemberDto>.Forbidden("You cannot update this staff member.");
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
        if (role is null || role.OrganizationId != staff.OrganizationId)
            return StaffResult<StaffMemberDto>.NotFound("Staff role was not found for this organization.");

        var employeeNumber = request.EmployeeNumber.Trim();
        if (await staffRepository.EmployeeNumberExistsAsync(staff.PropertyId, employeeNumber, staff.Id, cancellationToken))
            return StaffResult<StaffMemberDto>.Conflict("A staff member with this employee number already exists.");

        await staffRepository.UpdateAsync(
            staff.Id,
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

        var detail = await staffRepository.GetDetailAsync(request.StaffUid, cancellationToken);
        return detail is null
            ? StaffResult<StaffMemberDto>.NotFound("Staff member was not found.")
            : StaffResult<StaffMemberDto>.Success(detail);
    }
}
