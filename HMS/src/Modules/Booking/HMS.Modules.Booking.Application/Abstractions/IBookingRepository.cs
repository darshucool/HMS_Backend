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

public sealed record RatePlanBookingContext(
    long Id,
    long AccommodationTypeId,
    string PricingBasis);

public sealed record RatePlanPriceRow(
    DateOnly StartDate,
    DateOnly EndDate,
    int? DayOfWeek,
    decimal UnitRate,
    decimal? AdultRate,
    decimal? ChildRate,
    int MinimumStay);

public sealed record BookingUnitContext(
    long Id,
    long AccommodationTypeId,
    long? UnitId,
    string AllocationStatus);

public sealed record BookingUnitLine(
    long AccommodationTypeId,
    long? UnitId,
    long? RatePlanId,
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

public sealed record BookingGuestLine(
    Guid GuestUid,
    long GuestId,
    long? BookingUnitId,
    bool IsLeadGuest);

public sealed record BookingGuestContext(
    long Id,
    long OrganizationId,
    long BookingId,
    long GuestId,
    long? BookingUnitId,
    bool IsLeadGuest);   

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
    Task<RatePlanBookingContext?> GetRatePlanAsync(
        Guid ratePlanUid,
        long propertyId,
        long accommodationTypeId,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<RatePlanPriceRow>> GetRatePlanPricesAsync(
        long ratePlanId,
        DateOnly checkInDate,
        DateOnly checkOutDate,
        CancellationToken cancellationToken);
    Task<BookingUnitContext?> GetBookingUnitAsync(
        long bookingId,
        Guid bookingUnitUid,
        CancellationToken cancellationToken);
    Task<BookingGuestContext?> GetBookingGuestAsync(
        long bookingId,
        long guestId,
        CancellationToken cancellationToken);    
    Task InsertAsync(
        BookingEntity booking,
        IReadOnlyList<BookingUnitLine> units,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<DTOs.BookingDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
    Task<BookingCalendarDto> GetBookingCalendarAsync(
        long propertyId,
        Guid propertyUid,
        DateOnly from,
        DateOnly to,
        long? accommodationTypeId,
        CancellationToken cancellationToken);    
    Task<BookingEntity?> GetByUidAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task<DTOs.BookingDetailDto?> GetDetailByUidAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task UpdateAsync(BookingEntity booking, CancellationToken cancellationToken);
    Task UpdateGuestTypeAsync(
        long guestId,
        string guestType,
        string actorSubject,
        CancellationToken cancellationToken);
    Task AssignUnitAsync(
        long bookingUnitId,
        long unitId,
        string allocationStatus,
        DateTimeOffset modifiedDate,
        string? modifiedBy,
        CancellationToken cancellationToken);
    Task<BookingGuestDto> AddGuestAsync(
        BookingEntity booking,
        Guid guestUid,
        long guestId,
        long? bookingUnitId,
        bool isLeadGuest,
        string actorSubject,
        CancellationToken cancellationToken);
    Task RemoveGuestAsync(
        long bookingGuestId,
        string actorSubject,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<BookingStatusHistoryDto>> GetStatusHistoryAsync(
        long bookingId,
        CancellationToken cancellationToken);
}
