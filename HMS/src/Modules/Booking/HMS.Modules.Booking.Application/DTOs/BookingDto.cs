namespace HMS.Modules.Booking.Application.DTOs;

public sealed record BookingDto(
    Guid Uid,
    Guid PropertyUid,
    string BookingNumber,
    Guid? LeadGuestUid,
    string? LeadGuestName,
    string BookingSource,
    string Status,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    int Adults,
    int Children,
    int Infants,
    string Currency,
    decimal? QuotedTotal,
    string? SpecialRequests,
    DateTimeOffset CreationDate);

public sealed record BookingUnitDto(
    Guid Uid,
    Guid AccommodationTypeUid,
    Guid? UnitUid,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    int UnitQuantity,
    int GuestCount,
    string PricingBasis,
    decimal UnitRate,
    decimal TotalAmount,
    string AllocationStatus);

public sealed record BookingGuestDto(
    Guid GuestUid,
    string DisplayName,
    bool IsLeadGuest);

public sealed record BookingStatusHistoryDto(
    Guid Uid,
    string? OldStatus,
    string NewStatus,
    string? Reason,
    DateTimeOffset ChangedAt,
    string? ChangedBy);

public sealed record BookingHistoryDto(
    Guid BookingUid,
    string BookingNumber,
    string CurrentStatus,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    IReadOnlyList<BookingStatusHistoryDto> StatusChanges);

public sealed record BookingDetailDto(
    Guid Uid,
    Guid PropertyUid,
    string BookingNumber,
    Guid? LeadGuestUid,
    string? LeadGuestName,
    string BookingSource,
    string? ExternalReference,
    string Status,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    int Adults,
    int Children,
    int Infants,
    string Currency,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal? QuotedTotal,
    TimeOnly? ArrivalTime,
    TimeOnly? DepartureTime,
    string? SpecialRequests,
    string? InternalNotes,
    string? CancellationReason,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? CheckedInAt,
    DateTimeOffset? CheckedOutAt,
    DateTimeOffset? CancelledAt,
    DateTimeOffset CreationDate,
    IReadOnlyList<BookingUnitDto> Units,
    IReadOnlyList<BookingGuestDto> Guests);

