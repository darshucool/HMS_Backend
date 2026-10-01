using HMS.Modules.Finance.Application.DTOs;

namespace HMS.Modules.Finance.Application.Abstractions;

public sealed record UtilityTypeContext(
    long Id,
    Guid Uid,
    long OrganizationId,
    bool IsArchived);

public interface IUtilityTypeRepository
{
    Task<IReadOnlyList<UtilityTypeDto>> GetByOrganizationIdAsync(
        long organizationId,
        CancellationToken cancellationToken);

    Task<UtilityTypeContext?> GetByUidAsync(
        Guid utilityTypeUid,
        long organizationId,
        CancellationToken cancellationToken);

    Task<UtilityTypeDto?> GetDetailAsync(
        Guid utilityTypeUid,
        long organizationId,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        long organizationId,
        string code,
        Guid? excludeUid,
        CancellationToken cancellationToken);

    Task<Guid> InsertAsync(
        long organizationId,
        string code,
        string name,
        string? unitOfMeasure,
        bool isMetered,
        string actorSubject,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        long utilityTypeId,
        string code,
        string name,
        string? unitOfMeasure,
        bool isMetered,
        string actorSubject,
        CancellationToken cancellationToken);

    Task ArchiveAsync(long utilityTypeId, string actorSubject, CancellationToken cancellationToken);
}
