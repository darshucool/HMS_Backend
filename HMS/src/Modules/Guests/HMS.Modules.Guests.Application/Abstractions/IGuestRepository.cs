using HMS.Modules.Guests.Application.DTOs;
using HMS.Modules.Guests.Domain.Entities;

namespace HMS.Modules.Guests.Application.Abstractions;

public interface IGuestRepository
{
    Task InsertAsync(Guest guest, CancellationToken cancellationToken);
    Task<IReadOnlyList<GuestDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        string? search,
        CancellationToken cancellationToken);
}
