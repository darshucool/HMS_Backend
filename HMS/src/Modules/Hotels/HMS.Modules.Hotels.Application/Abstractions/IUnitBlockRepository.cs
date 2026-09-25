using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IUnitBlockRepository
{
    Task<UnitBlock?> GetByUidAsync(Guid uid, CancellationToken cancellationToken);
    Task<bool> OverlapsAsync(
        long unitId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);
    Task InsertAsync(UnitBlock block, CancellationToken cancellationToken);
    Task ArchiveAsync(UnitBlock block, CancellationToken cancellationToken);
}
