using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IRatePlanRepository
{
    Task<RatePlan?> GetByUidAsync(Guid uid, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(long propertyId, string code, CancellationToken cancellationToken);
    Task InsertAsync(RatePlan ratePlan, CancellationToken cancellationToken);
    Task<IReadOnlyList<RatePlanDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
}
