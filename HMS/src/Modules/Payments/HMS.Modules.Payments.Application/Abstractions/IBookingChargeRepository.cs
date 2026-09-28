using HMS.Modules.Payments.Application.DTOs;

namespace HMS.Modules.Payments.Application.Abstractions;

public sealed record ChargeTypeRef(long Id, Guid Uid, string Name, long OrganizationId, long PropertyId);

public sealed record BookingChargeContext(
    long Id,
    Guid Uid,
    long OrganizationId,
    long PropertyId,
    Guid PropertyUid,
    long BookingId,
    bool IsArchived);

public interface IBookingChargeRepository
{
    Task<ChargeTypeRef?> GetChargeTypeAsync(Guid chargeTypeUid, CancellationToken cancellationToken);
    Task<IReadOnlyList<BookingChargeDto>> GetByBookingIdAsync(long bookingId, CancellationToken cancellationToken);
    Task<BookingChargeContext?> GetByUidAsync(Guid chargeUid, CancellationToken cancellationToken);
    Task<Guid> InsertAsync(
        long organizationId,
        long propertyId,
        long bookingId,
        long? bookingUnitId,
        long chargeTypeId,
        DateOnly serviceDate,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxAmount,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken);
    Task UpdateAsync(
        long chargeId,
        long chargeTypeId,
        long? bookingUnitId,
        DateOnly serviceDate,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxAmount,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken);
    Task DeleteAsync(long chargeId, string actorSubject, CancellationToken cancellationToken);
    Task<BookingChargeDto?> GetDetailByUidAsync(Guid chargeUid, CancellationToken cancellationToken);
}
