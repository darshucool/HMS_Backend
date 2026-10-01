using HMS.Modules.Staff.Application.DTOs;

namespace HMS.Modules.Staff.Application.Abstractions;

public sealed record StaffPropertyContext(long Id, long OrganizationId, Guid Uid, bool IsArchived);

public sealed record StaffRoleRef(long Id, long OrganizationId, string Name);

public sealed record StaffMemberContext(
    long Id,
    Guid Uid,
    long OrganizationId,
    long PropertyId,
    Guid PropertyUid,
    bool IsArchived);

public interface IStaffPropertyAccess
{
    Task<StaffPropertyContext?> GetPropertyAsync(Guid propertyUid, CancellationToken cancellationToken);
    Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken);
}

public interface IStaffRepository
{
    Task<StaffRoleRef?> GetRoleAsync(Guid staffRoleUid, CancellationToken cancellationToken);
    Task<long?> GetBookingIdAsync(Guid bookingUid, long propertyId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StaffMemberDto>> GetByPropertyAsync(long propertyId, CancellationToken cancellationToken);
    Task<StaffMemberContext?> GetContextAsync(Guid staffUid, CancellationToken cancellationToken);
    Task<StaffMemberDto?> GetDetailAsync(Guid staffUid, CancellationToken cancellationToken);
    Task<bool> EmployeeNumberExistsAsync(
        long propertyId,
        string employeeNumber,
        long? excludeStaffId,
        CancellationToken cancellationToken);
    Task<Guid> InsertAsync(
        long organizationId,
        long propertyId,
        long staffRoleId,
        string employeeNumber,
        string firstName,
        string? lastName,
        string? phone,
        string? email,
        string employmentType,
        decimal? basicSalary,
        decimal? dailyRate,
        decimal? hourlyRate,
        DateOnly? joinedDate,
        DateOnly? leftDate,
        string status,
        string actorSubject,
        CancellationToken cancellationToken);
    Task UpdateAsync(
        long staffId,
        long staffRoleId,
        string employeeNumber,
        string firstName,
        string? lastName,
        string? phone,
        string? email,
        string employmentType,
        decimal? basicSalary,
        decimal? dailyRate,
        decimal? hourlyRate,
        DateOnly? joinedDate,
        DateOnly? leftDate,
        string status,
        string actorSubject,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<StaffWorkLogDto>> GetWorkLogsAsync(long staffId, CancellationToken cancellationToken);
    Task<Guid> InsertWorkLogAsync(
        long organizationId,
        long propertyId,
        long staffId,
        long? bookingId,
        DateOnly workDate,
        TimeOnly? startTime,
        TimeOnly? endTime,
        decimal hoursWorked,
        decimal overtimeHours,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken);
    Task<StaffWorkLogDto?> GetWorkLogAsync(Guid workLogUid, CancellationToken cancellationToken);
    Task<IReadOnlyList<StaffPaymentDto>> GetPaymentsByPropertyAsync(long propertyId, CancellationToken cancellationToken);
    Task<Guid> InsertPaymentAsync(
        long organizationId,
        long propertyId,
        long staffId,
        DateOnly periodStart,
        DateOnly periodEnd,
        decimal basicAmount,
        decimal overtimeAmount,
        decimal bonusAmount,
        decimal deductionAmount,
        decimal totalPaid,
        string paymentMethod,
        DateTimeOffset paidAt,
        string? referenceNumber,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken);
    Task<StaffPaymentDto?> GetPaymentAsync(Guid paymentUid, CancellationToken cancellationToken);
}
