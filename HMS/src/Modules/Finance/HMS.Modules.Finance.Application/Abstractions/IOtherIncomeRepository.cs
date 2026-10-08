using HMS.Modules.Finance.Application.DTOs;
using OtherIncomeEntity = HMS.Modules.Finance.Domain.Entities.OtherIncome;

namespace HMS.Modules.Finance.Application.Abstractions;

public sealed record IncomeCategoryRef(long Id, Guid Uid, string Name, long OrganizationId);

public sealed record OtherIncomeContext(
    long Id,
    Guid Uid,
    long OrganizationId,
    long PropertyId,
    Guid PropertyUid,
    bool IsActive,
    bool IsArchived,
    DateTimeOffset CreationDate,
    string? CreatedBy);

public interface IOtherIncomeRepository
{
    Task<IncomeCategoryRef?> GetCategoryAsync(Guid categoryUid, CancellationToken cancellationToken);
    Task<BookingRef?> GetBookingAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task InsertAsync(OtherIncomeEntity income, CancellationToken cancellationToken);
    Task<IReadOnlyList<OtherIncomeDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
    Task<OtherIncomeDto?> GetByUidAsync(
        Guid propertyUid,
        Guid otherIncomeUid,
        CancellationToken cancellationToken);
    Task<OtherIncomeContext?> GetContextAsync(
        Guid propertyUid,
        Guid otherIncomeUid,
        CancellationToken cancellationToken);
    Task UpdateAsync(OtherIncomeEntity income, CancellationToken cancellationToken);
    Task ArchiveAsync(OtherIncomeEntity income, CancellationToken cancellationToken);
}
