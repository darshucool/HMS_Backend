using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IAccommodationTypeRepository
{
    Task<AccommodationType?> GetByUidAsync(Guid uid, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(
        long propertyId,
        string code,
        Guid? excludeUid,
        CancellationToken cancellationToken);
    Task InsertAsync(AccommodationType accommodationType, CancellationToken cancellationToken);
    Task UpdateAsync(AccommodationType accommodationType, CancellationToken cancellationToken);
    Task ArchiveAsync(AccommodationType accommodationType, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccommodationTypeDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
}
