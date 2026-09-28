using HMS.Modules.Booking.Domain.Enums;

namespace HMS.Modules.Booking.Api.Contracts;

public sealed record CreateBookingUnitRequest(
    Guid AccommodationTypeUid,
    PricingBasis PricingBasis,
    decimal UnitRate,
    Guid? UnitUid = null,
    Guid? RatePlanUid = null,
    Guid? MealPlanUid = null,
    int Adults = 1,
    int Children = 0,
    int UnitQuantity = 1,
    int GuestCount = 1,
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    string? Notes = null);

public sealed record CreateBookingRequest(
    Guid LeadGuestUid,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    BookingSource BookingSource = BookingSource.Direct,
    int Adults = 1,
    int Children = 0,
    int Infants = 0,
    string Currency = "LKR",
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    decimal ServiceCharge = 0,
    TimeOnly? ArrivalTime = null,
    TimeOnly? DepartureTime = null,
    string? SpecialRequests = null,
    string? InternalNotes = null,
    string? ExternalReference = null,
    IReadOnlyList<CreateBookingUnitRequest>? Units = null);

public sealed record UpdateBookingRequest(
    Guid LeadGuestUid,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    BookingSource BookingSource = BookingSource.Direct,
    BookingStatus Status = BookingStatus.Pending,
    int Adults = 1,
    int Children = 0,
    int Infants = 0,
    string Currency = "LKR",
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    decimal ServiceCharge = 0,
    decimal? QuotedTotal = null,
    TimeOnly? ArrivalTime = null,
    TimeOnly? DepartureTime = null,
    string? SpecialRequests = null,
    string? InternalNotes = null,
    string? ExternalReference = null,
    string? CancellationReason = null);

public sealed record CancelBookingRequest(string Reason);

public sealed record AssignBookingUnitRequest(
    Guid BookingUnitUid,
    Guid UnitUid);

public sealed record AddBookingGuestRequest(
    Guid GuestUid,
    Guid? BookingUnitUid = null,
    bool IsLeadGuest = false);