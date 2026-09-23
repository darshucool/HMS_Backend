using HMS.Modules.Guests.Domain.Common;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Domain.Entities;

public sealed class Guest : AuditableEntity
{
    private Guest() { }

    public long OrganizationId { get; private set; }
    public Guid OrganizationUid { get; private set; }
    public GuestType GuestType { get; private set; } = GuestType.Individual;
    public string? Title { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? AlternatePhone { get; private set; }
    public string? Email { get; private set; }
    public string? NationalityCode { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? CountryCode { get; private set; }
    public string? Notes { get; private set; }

    public static Guest Create(
        long organizationId,
        Guid organizationUid,
        GuestType guestType,
        string? title,
        string? firstName,
        string? lastName,
        string? displayName,
        string? phone,
        string? alternatePhone,
        string? email,
        string? nationalityCode,
        DateOnly? dateOfBirth,
        string? preferredLanguage,
        string? address,
        string? city,
        string? countryCode,
        string? notes,
        string actorSubject)
    {
        var cleanedFirstName = Optional(firstName, nameof(firstName), 100);
        var cleanedLastName = Optional(lastName, nameof(lastName), 100);
        var resolvedDisplayName = Optional(displayName, nameof(displayName), 200)
            ?? BuildDisplayName(cleanedFirstName, cleanedLastName);

        if (resolvedDisplayName is null)
            throw new ArgumentException("Display name or first/last name is required.", nameof(displayName));

        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));

        var guest = new Guest
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            OrganizationUid = organizationUid,
            GuestType = guestType,
            Title = Optional(title, nameof(title), 20),
            FirstName = cleanedFirstName,
            LastName = cleanedLastName,
            DisplayName = resolvedDisplayName,
            Phone = Optional(phone, nameof(phone), 30),
            AlternatePhone = Optional(alternatePhone, nameof(alternatePhone), 30),
            Email = OptionalEmail(email),
            NationalityCode = OptionalCountryCode(nationalityCode, nameof(nationalityCode)),
            DateOfBirth = dateOfBirth,
            PreferredLanguage = Optional(preferredLanguage, nameof(preferredLanguage), 10),
            Address = Optional(address, nameof(address), 2000),
            City = Optional(city, nameof(city), 100),
            CountryCode = OptionalCountryCode(countryCode, nameof(countryCode)),
            Notes = Optional(notes, nameof(notes), 4000)
        };

        guest.MarkCreated(actorSubject);
        return guest;
    }

    public static Guest Rehydrate(
        long id,
        Guid uid,
        long organizationId,
        Guid organizationUid,
        GuestType guestType,
        string? title,
        string? firstName,
        string? lastName,
        string displayName,
        string? phone,
        string? alternatePhone,
        string? email,
        string? nationalityCode,
        DateOnly? dateOfBirth,
        string? preferredLanguage,
        string? address,
        string? city,
        string? countryCode,
        string? notes,
        bool isActive,
        bool isArchived,
        DateTimeOffset creationDate,
        string? createdBy,
        DateTimeOffset? modifiedDate,
        string? modifiedBy) =>
        new()
        {
            Id = id,
            Uid = uid,
            OrganizationId = organizationId,
            OrganizationUid = organizationUid,
            GuestType = guestType,
            Title = title,
            FirstName = firstName,
            LastName = lastName,
            DisplayName = displayName,
            Phone = phone,
            AlternatePhone = alternatePhone,
            Email = email,
            NationalityCode = nationalityCode,
            DateOfBirth = dateOfBirth,
            PreferredLanguage = preferredLanguage,
            Address = address,
            City = city,
            CountryCode = countryCode,
            Notes = notes,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };

    private static string? BuildDisplayName(string? firstName, string? lastName)
    {
        var parts = new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part));
        var combined = string.Join(' ', parts);
        return string.IsNullOrWhiteSpace(combined) ? null : combined;
    }

    private static string? Optional(string? value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", name);

        return trimmed;
    }

    private static string? OptionalEmail(string? email)
    {
        var cleaned = Optional(email, nameof(email), 254);
        if (cleaned is null)
            return null;

        if (!cleaned.Contains('@'))
            throw new ArgumentException("Email is invalid.", nameof(email));

        return cleaned;
    }

    private static string? OptionalCountryCode(string? value, string name)
    {
        var cleaned = Optional(value, name, 2);
        if (cleaned is null)
            return null;

        if (cleaned.Length != 2)
            throw new ArgumentException($"{name} must be a 2-letter code.", name);

        return cleaned.ToUpperInvariant();
    }
}
