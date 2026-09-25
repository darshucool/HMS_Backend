using HMS.Modules.Booking.Application.DTOs;
using BookingEntity = HMS.Modules.Booking.Domain.Entities.Booking;

namespace HMS.Modules.Booking.Application.Abstractions;

public sealed record PropertyBookingContext(
    long Id,
    long OrganizationId,
    string BookingNumberPrefix,
    bool IsArchived);

public sealed record GuestBookingContext(long Id, long OrganizationId);

public sealed record AccommodationTypeBookingContext(long Id);

public sealed record BookingUnitContext(
    long Id,
    long AccommodationTypeId,
    long? UnitId,
    string AllocationStatus);

public sealed record BookingUnitLine(
    long AccommodationTypeId,
    long? UnitId,
    long? RatePlanId,
    long? MealPlanId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    int UnitQuantity,
    int GuestCount,
    string PricingBasis,
    decimal UnitRate,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string? Notes);

public interface IPropertyBookingAccess
{
    Task<PropertyBookingContext?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken);
    Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken);
}

public interface IBookingRepository
{
    Task<string> NextBookingNumberAsync(
        long propertyId,
        string prefix,
        CancellationToken cancellationToken);
    Task<GuestBookingContext?> GetGuestAsync(Guid guestUid, CancellationToken cancellationToken);
    Task<AccommodationTypeBookingContext?> GetAccommodationTypeAsync(
        Guid accommodationTypeUid,
        long propertyId,
        CancellationToken cancellationToken);
    Task<long?> GetUnitIdAsync(
        Guid unitUid,
        long propertyId,
        long accommodationTypeId,
        CancellationToken cancellationToken);
    Task<long?> GetRatePlanIdAsync(Guid ratePlanUid, long propertyId, CancellationToken cancellationToken);
    Task<long?> GetMealPlanIdAsync(Guid mealPlanUid, long propertyId, CancellationToken cancellationToken);
    Task<BookingUnitContext?> GetBookingUnitAsync(
        long bookingId,
        Guid bookingUnitUid,
        CancellationToken cancellationToken);
    Task InsertAsync(
        BookingEntity booking,
        IReadOnlyList<BookingUnitLine> units,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<DTOs.BookingDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
    Task<BookingEntity?> GetByUidAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task<DTOs.BookingDetailDto?> GetDetailByUidAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task UpdateAsync(BookingEntity booking, CancellationToken cancellationToken);
    Task AssignUnitAsync(
        long bookingUnitId,
        long unitId,
        string allocationStatus,
        DateTimeOffset modifiedDate,
        string? modifiedBy,
        CancellationToken cancellationToken);
}
