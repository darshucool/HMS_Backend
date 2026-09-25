using HMS.Modules.Finance.Application.DTOs;
using OtherIncomeEntity = HMS.Modules.Finance.Domain.Entities.OtherIncome;

namespace HMS.Modules.Finance.Application.Abstractions;

public sealed record IncomeCategoryRef(long Id, Guid Uid, string Name, long OrganizationId);

public interface IOtherIncomeRepository
{
    Task<IncomeCategoryRef?> GetCategoryAsync(Guid categoryUid, CancellationToken cancellationToken);
    Task<BookingRef?> GetBookingAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task InsertAsync(OtherIncomeEntity income, CancellationToken cancellationToken);
    Task<IReadOnlyList<OtherIncomeDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
}
