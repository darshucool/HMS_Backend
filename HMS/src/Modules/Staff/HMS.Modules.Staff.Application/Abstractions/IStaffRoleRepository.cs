using HMS.Modules.Staff.Application.DTOs;

namespace HMS.Modules.Staff.Application.Abstractions;

public enum StaffRoleDeleteStatus
{
    Deleted,
    NotFound,
    InUse
}

public interface IStaffRoleRepository
{
    Task<bool> CodeExistsAsync(long organizationId, string code, CancellationToken cancellationToken);
    Task<IReadOnlyList<StaffRoleDto>> ListAsync(long organizationId, CancellationToken cancellationToken);
    Task<StaffRoleDto> InsertAsync(
        long organizationId,
        string code,
        string name,
        string actorSubject,
        CancellationToken cancellationToken);
    Task<StaffRoleDeleteStatus> ArchiveAsync(
        Guid staffRoleUid,
        long organizationId,
        string actorSubject,
        CancellationToken cancellationToken);
}
