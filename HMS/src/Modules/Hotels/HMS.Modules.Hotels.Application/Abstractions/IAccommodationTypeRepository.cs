using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IAccommodationTypeRepository
{
    Task<bool> CodeExistsAsync(long propertyId, string code, CancellationToken cancellationToken);
    Task InsertAsync(AccommodationType accommodationType, CancellationToken cancellationToken);
    Task<IReadOnlyList<AccommodationTypeDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
}
