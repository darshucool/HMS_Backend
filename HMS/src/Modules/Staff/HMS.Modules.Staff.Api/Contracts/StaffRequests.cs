using HMS.Modules.Staff.Domain.Enums;

namespace HMS.Modules.Staff.Api.Contracts;

public sealed record UpsertStaffMemberRequest(
    Guid StaffRoleUid,
    string EmployeeNumber,
    string FirstName,
    EmploymentType EmploymentType,
    string? LastName = null,
    string? Phone = null,
    string? Email = null,
    decimal? BasicSalary = null,
    decimal? DailyRate = null,
    decimal? HourlyRate = null,
    DateOnly? JoinedDate = null,
    DateOnly? LeftDate = null,
    StaffMemberStatus Status = StaffMemberStatus.Active);

public sealed record CreateStaffWorkLogRequest(
    DateOnly WorkDate,
    decimal HoursWorked = 0,
    decimal OvertimeHours = 0,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    Guid? BookingUid = null,
    string? Notes = null);

public sealed record CreateStaffPaymentRequest(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    StaffPaymentMethod PaymentMethod,
    decimal BasicAmount = 0,
    decimal OvertimeAmount = 0,
    decimal BonusAmount = 0,
    decimal DeductionAmount = 0,
    DateTimeOffset? PaidAt = null,
    string? ReferenceNumber = null,
    string? Notes = null);
