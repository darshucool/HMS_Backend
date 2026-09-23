using HMS.Modules.Guests.Application.DTOs;
using HMS.Modules.Guests.Domain.Entities;

namespace HMS.Modules.Guests.Application.Abstractions;

public interface IGuestRepository
{
    Task InsertAsync(Guest guest, CancellationToken cancellationToken);
    Task UpdateAsync(Guest guest, CancellationToken cancellationToken);
    Task<Guest?> GetByUidAsync(Guid uid, CancellationToken cancellationToken);
    Task<IReadOnlyList<GuestDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        string? search,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<GuestDto>> SearchAsync(
        string actorSubject,
        bool isPlatformAdmin,
        string? phone,
        string? name,
        string? email,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<GuestBookingHistoryDto>> GetBookingHistoryAsync(
        Guid guestUid,
        CancellationToken cancellationToken);
}
