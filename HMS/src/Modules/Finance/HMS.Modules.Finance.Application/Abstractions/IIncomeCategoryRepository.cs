using HMS.Modules.Finance.Application.DTOs;

namespace HMS.Modules.Finance.Application.Abstractions;

public sealed record IncomeCategoryContext(long Id, Guid Uid, bool IsArchived);

public interface IIncomeCategoryRepository
{
    Task<IReadOnlyList<IncomeCategoryDto>> ListAsync(long organizationId, CancellationToken cancellationToken);

    Task<IncomeCategoryDto?> GetDetailAsync(
        Guid categoryUid,
        long organizationId,
        CancellationToken cancellationToken);

    Task<IncomeCategoryContext?> GetContextAsync(
        Guid categoryUid,
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
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        long categoryId,
        string code,
        string name,
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken);

    Task ArchiveAsync(long categoryId, string actorSubject, CancellationToken cancellationToken);
}
