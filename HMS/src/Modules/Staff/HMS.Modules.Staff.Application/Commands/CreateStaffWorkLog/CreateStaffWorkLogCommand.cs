using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using MediatR;

namespace HMS.Modules.Staff.Application.Commands.CreateStaffWorkLog;

public sealed record CreateStaffWorkLogCommand(
    Guid StaffUid,
    DateOnly WorkDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    decimal HoursWorked,
    decimal OvertimeHours,
    Guid? BookingUid,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<StaffWorkLogDto>>;

public sealed class CreateStaffWorkLogCommandHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<CreateStaffWorkLogCommand, StaffResult<StaffWorkLogDto>>
{
    public async Task<StaffResult<StaffWorkLogDto>> Handle(
        CreateStaffWorkLogCommand request,
        CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetContextAsync(request.StaffUid, cancellationToken);
        if (staff is null || staff.IsArchived)
            return StaffResult<StaffWorkLogDto>.NotFound("Staff member was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, staff.PropertyUid, true, cancellationToken))
        {
            return StaffResult<StaffWorkLogDto>.Forbidden("You cannot add work logs for this staff member.");
        }

        if (request.HoursWorked < 0 || request.OvertimeHours < 0)
            return StaffResult<StaffWorkLogDto>.Validation("Hours cannot be negative.");

        long? bookingId = null;
        if (request.BookingUid is Guid bookingUid)
        {
            bookingId = await staffRepository.GetBookingIdAsync(bookingUid, staff.PropertyId, cancellationToken);
            if (bookingId is null)
                return StaffResult<StaffWorkLogDto>.NotFound("Booking was not found for this property.");
        }

        var uid = await staffRepository.InsertWorkLogAsync(
            staff.OrganizationId,
            staff.PropertyId,
            staff.Id,
            bookingId,
            request.WorkDate,
            request.StartTime,
            request.EndTime,
            request.HoursWorked,
            request.OvertimeHours,
            StaffProfileRules.TrimOrNull(request.Notes),
            request.ActorSubject,
            cancellationToken);

        var detail = await staffRepository.GetWorkLogAsync(uid, cancellationToken);
        return detail is null
            ? StaffResult<StaffWorkLogDto>.NotFound("Work log was not found.")
            : StaffResult<StaffWorkLogDto>.Success(detail);
    }
}
