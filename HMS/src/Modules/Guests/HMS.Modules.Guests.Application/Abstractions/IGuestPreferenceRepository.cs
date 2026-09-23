using HMS.Modules.Guests.Domain.Entities;

namespace HMS.Modules.Guests.Application.Abstractions;

public interface IGuestPreferenceRepository
{
    Task InsertAsync(GuestPreference preference, CancellationToken cancellationToken);
}
