namespace HMS.Modules.Guests.Application.DTOs;

public sealed record GuestDto(
    Guid Uid,
    Guid OrganizationUid,
    string GuestType,
    string? Title,
    string? FirstName,
    string? LastName,
    string DisplayName,
    string? Phone,
    string? AlternatePhone,
    string? Email,
    string? NationalityCode,
    DateOnly? DateOfBirth,
    string? PreferredLanguage,
    string? Address,
    string? City,
    string? CountryCode,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record GuestDocumentDto(
    Guid Uid,
    Guid GuestUid,
    string DocumentType,
    string DocumentNumber,
    string? IssuingCountry,
    DateOnly? IssuedDate,
    DateOnly? ExpiryDate,
    string? FileUrl,
    bool IsVerified,
    DateTimeOffset CreationDate);

public sealed record GuestPreferenceDto(
    Guid Uid,
    Guid GuestUid,
    string PreferenceType,
    string PreferenceKey,
    string PreferenceValue,
    string? Notes,
    DateTimeOffset CreationDate);

public sealed record GuestBookingHistoryDto(
    Guid BookingUid,
    string BookingNumber,
    Guid PropertyUid,
    string PropertyName,
    string Status,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    decimal? QuotedTotal,
    string Currency,
    bool IsLeadGuest,
    DateTimeOffset CreationDate);
