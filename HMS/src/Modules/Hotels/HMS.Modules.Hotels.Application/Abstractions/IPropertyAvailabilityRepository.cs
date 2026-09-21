using HMS.Modules.Hotels.Application.DTOs;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IPropertyAvailabilityRepository
{
    Task<IReadOnlyList<PropertyAvailabilityItemDto>> GetAsync(
        Guid propertyUid,
        DateOnly checkIn,
        DateOnly checkOut,
        int adults,
        int children,
        CancellationToken cancellationToken);
}
