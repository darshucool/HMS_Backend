using HMS.Modules.Identity.Domain.Entities;

namespace HMS.Modules.Identity.Application.Abstractions;

public sealed record PropertyLookup(
    long Id,
    long OrganizationId,
    Guid Uid,
    bool IsArchived);

public interface IStaffRepository
{
    Task<StaffLoginRecord?> GetForLoginAsync(
        string normalizedUsername,
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task RecordSuccessfulLoginAsync(
        Guid staffUid,
        CancellationToken cancellationToken);

    Task RecordFailedLoginAsync(
        Guid staffUid,
        int maximumFailedAttempts,
        TimeSpan lockDuration,
        CancellationToken cancellationToken);

    Task<bool> UsernameExistsAsync(
        string normalizedUsername,
        CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(
        string normalizedEmail,
        CancellationToken cancellationToken);

    Task<bool> RoleExistsAsync(
        string roleCode,
        CancellationToken cancellationToken);

    Task<PropertyLookup?> GetPropertyByUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task InsertRegisteredUserAsync(
        StaffUser staff,
        Guid propertyUid,
        string identityRoleCode,
        string hotelAccessRoleCode,
        Guid? createdBy,
        CancellationToken cancellationToken);
}
