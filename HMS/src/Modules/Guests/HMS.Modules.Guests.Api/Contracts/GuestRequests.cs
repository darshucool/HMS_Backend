using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Api.Contracts;

public sealed record CreateGuestRequest(
    string? DisplayName = null,
    string? FirstName = null,
    string? LastName = null,
    string? Title = null,
    GuestType GuestType = GuestType.Individual,
    string? Phone = null,
    string? AlternatePhone = null,
    string? Email = null,
    string? NationalityCode = null,
    DateOnly? DateOfBirth = null,
    string? PreferredLanguage = null,
    string? Address = null,
    string? City = null,
    string? CountryCode = null,
    string? Notes = null);

public sealed record UpdateGuestRequest(
    string? DisplayName = null,
    string? FirstName = null,
    string? LastName = null,
    string? Title = null,
    GuestType GuestType = GuestType.Individual,
    string? Phone = null,
    string? AlternatePhone = null,
    string? Email = null,
    string? NationalityCode = null,
    DateOnly? DateOfBirth = null,
    string? PreferredLanguage = null,
    string? Address = null,
    string? City = null,
    string? CountryCode = null,
    string? Notes = null,
    bool IsActive = true);

public sealed record CreateGuestDocumentRequest(
    string DocumentNumber,
    DocumentType DocumentType = DocumentType.Nic,
    string? IssuingCountry = null,
    DateOnly? IssuedDate = null,
    DateOnly? ExpiryDate = null,
    string? FileUrl = null);

public sealed record CreateGuestPreferenceRequest(
    string PreferenceKey,
    string PreferenceValue,
    PreferenceType PreferenceType = PreferenceType.Room,
    string? Notes = null);
