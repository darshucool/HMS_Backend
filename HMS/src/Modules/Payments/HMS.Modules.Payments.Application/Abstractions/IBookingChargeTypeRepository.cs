using HMS.Modules.Payments.Application.DTOs;

namespace HMS.Modules.Payments.Application.Abstractions;

public sealed record BookingChargeTypeContext(
    long Id,
    Guid Uid,
    Guid PropertyUid,
    bool IsArchived);

public interface IBookingChargeTypeRepository
{
    Task<IReadOnlyList<BookingChargeTypeDto>> ListAsync(
        long propertyId,
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task<BookingChargeTypeContext?> GetByUidAsync(
        Guid chargeTypeUid,
        long propertyId,
        CancellationToken cancellationToken);

    Task<BookingChargeTypeDto?> GetDetailAsync(
        Guid chargeTypeUid,
        long propertyId,
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        long propertyId,
        string code,
        Guid? excludeUid,
        CancellationToken cancellationToken);

    Task<Guid> InsertAsync(
        long organizationId,
        long propertyId,
        string code,
        string name,
        string category,
        bool isTaxable,
        decimal? defaultPrice,
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        long chargeTypeId,
        string code,
        string name,
        string category,
        bool isTaxable,
        decimal? defaultPrice,
        bool isActive,
        string actorSubject,
        CancellationToken cancellationToken);

    Task ArchiveAsync(long chargeTypeId, string actorSubject, CancellationToken cancellationToken);
}
