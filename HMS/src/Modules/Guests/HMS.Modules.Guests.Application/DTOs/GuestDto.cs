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
