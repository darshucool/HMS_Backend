using HMS.Modules.Reports.Application.DTOs;

namespace HMS.Modules.Reports.Application.Abstractions;

public interface IBookingRevenueRepository
{
    Task<IReadOnlyList<BookingRevenueDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
}
