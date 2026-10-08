namespace HMS.Modules.Booking.Application.DTOs;

public sealed record BookingCalendarDto(
    Guid PropertyUid,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<BookingCalendarUnitDto> Units);

public sealed record BookingCalendarUnitDto(
    Guid UnitUid,
    string UnitCode,
    string? UnitName,
    Guid AccommodationTypeUid,
    string AccommodationTypeName,
    IReadOnlyList<BookingCalendarSegmentDto> Segments);

public sealed record BookingCalendarSegmentDto(
    string SegmentType,
    Guid? BookingUid,
    string? BookingNumber,
    string? Label,
    string Status,
    DateOnly StartDate,
    DateOnly EndDate);
