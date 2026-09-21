using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IAccommodationUnitRepository
{
    Task<AccommodationUnit?> GetByUidAsync(Guid uid, CancellationToken cancellationToken);
    Task<bool> UnitCodeExistsAsync(
        long propertyId,
        string unitCode,
        Guid? excludeUid,
        CancellationToken cancellationToken);
    Task InsertAsync(AccommodationUnit unit, CancellationToken cancellationToken);
    Task UpdateAsync(AccommodationUnit unit, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccommodationUnitDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
}
