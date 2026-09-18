using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IOrganizationRepository
{
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<Organization?> GetByUidAsync(Guid uid, CancellationToken cancellationToken);
    Task InsertAsync(Organization organization, CancellationToken cancellationToken);
    Task<(IReadOnlyList<OrganizationDto> Items, long TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken);
}

