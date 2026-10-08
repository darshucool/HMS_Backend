namespace HMS.Modules.Payments.Application.Abstractions;

public sealed record PaymentBookingContext(
    long Id,
    long OrganizationId,
    long PropertyId,
    Guid PropertyUid,
    string Status,
    bool IsArchived);

public sealed record PaymentBookingUnitContext(long Id);

public interface IPaymentBookingLookup
{
    Task<PaymentBookingContext?> GetByUidAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task<PaymentBookingUnitContext?> GetBookingUnitAsync(
        long bookingId,
        Guid bookingUnitUid,
        CancellationToken cancellationToken);
}

public sealed record PaymentPropertyContext(long Id, long OrganizationId, Guid Uid, bool IsArchived);

public interface IPaymentPropertyAccess
{
    Task<PaymentPropertyContext?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken);

    Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken);
}
