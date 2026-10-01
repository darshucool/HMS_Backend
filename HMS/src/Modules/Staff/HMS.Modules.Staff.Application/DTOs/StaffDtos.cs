namespace HMS.Modules.Staff.Application.DTOs;

public sealed record StaffMemberDto(
    Guid Uid,
    Guid PropertyUid,
    Guid StaffRoleUid,
    string StaffRoleName,
    string EmployeeNumber,
    string FirstName,
    string? LastName,
    string? Phone,
    string? Email,
    string EmploymentType,
    decimal? BasicSalary,
    decimal? DailyRate,
    decimal? HourlyRate,
    DateOnly? JoinedDate,
    DateOnly? LeftDate,
    string Status,
    DateTimeOffset CreationDate);

public sealed record StaffWorkLogDto(
    Guid Uid,
    Guid StaffUid,
    Guid? BookingUid,
    DateOnly WorkDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    decimal HoursWorked,
    decimal OvertimeHours,
    string? Notes,
    DateTimeOffset CreationDate);

public sealed record StaffPaymentDto(
    Guid Uid,
    Guid PropertyUid,
    Guid StaffUid,
    string EmployeeNumber,
    string StaffName,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal BasicAmount,
    decimal OvertimeAmount,
    decimal BonusAmount,
    decimal DeductionAmount,
    decimal TotalPaid,
    string PaymentMethod,
    DateTimeOffset PaidAt,
    string? ReferenceNumber,
    string? Notes,
    DateTimeOffset CreationDate);
