using HMS.Modules.Identity.Domain.Entities;

namespace HMS.Modules.Identity.Application.Abstractions;

public sealed record PropertyLookup(
    long Id,
    long OrganizationId,
    Guid Uid,
    string Name,
    bool IsArchived);

public sealed record ExistingStaffAccount(
    Guid Uid,
    string Username,
    string FirstName,
    string? LastName,
    string? Email);

public interface IStaffRepository
{
    Task<StaffLoginRecord?> GetForLoginAsync(
        string normalizedEmail,
        Guid? propertyUid,
        CancellationToken cancellationToken);

    Task<StaffLoginRecord?> GetByUidAsync(
        Guid staffUid,
        CancellationToken cancellationToken);

    Task RecordSuccessfulLoginAsync(
        Guid staffUid,
        CancellationToken cancellationToken);

    Task RecordFailedLoginAsync(
        Guid staffUid,
        int maximumFailedAttempts,
        TimeSpan lockDuration,
        CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(
        string normalizedEmail,
        CancellationToken cancellationToken);

    Task<ExistingStaffAccount?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken);

    Task UpdatePasswordAsync(
        Guid staffUid,
        string passwordHash,
        CancellationToken cancellationToken);

    Task AssignPropertyAsync(
        Guid staffUid,
        Guid propertyUid,
        string identityRoleCode,
        string hotelAccessRoleCode,
        Guid? createdBy,
        CancellationToken cancellationToken);

    Task<bool> RoleExistsAsync(
        string roleCode,
        CancellationToken cancellationToken);

    Task EnsureRoleExistsAsync(
        string roleCode,
        string name,
        string? description,
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

    Task<IReadOnlyList<PropertyAdminRecord>> GetPropertyAdminsAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task<PropertyAdminRecord?> GetPropertyAdminAsync(
        Guid propertyUid,
        Guid adminId,
        CancellationToken cancellationToken);

    Task<bool> UpdatePropertyAdminAsync(
        Guid propertyUid,
        Guid adminId,
        string firstName,
        string? lastName,
        bool isActive,
        CancellationToken cancellationToken);

    Task<bool> RemovePropertyAdminAsync(
        Guid propertyUid,
        Guid adminId,
        CancellationToken cancellationToken);
}

public sealed record PropertyAdminRecord(
    Guid AdminId,
    Guid PropertyUid,
    string? Email,
    string FirstName,
    string? LastName,
    bool IsActive,
    DateTimeOffset AssignedAt);
