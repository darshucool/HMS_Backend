using HMS.Modules.Guests.Domain.Common;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Domain.Entities;

public sealed class GuestPreference : AuditableEntity
{
    private GuestPreference() { }

    public long OrganizationId { get; private set; }
    public long GuestId { get; private set; }
    public Guid GuestUid { get; private set; }
    public PreferenceType PreferenceType { get; private set; }
    public string PreferenceKey { get; private set; } = string.Empty;
    public string PreferenceValue { get; private set; } = string.Empty;
    public string? Notes { get; private set; }

    public static GuestPreference Create(
        long organizationId,
        long guestId,
        Guid guestUid,
        PreferenceType preferenceType,
        string preferenceKey,
        string preferenceValue,
        string? notes,
        string actorSubject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(preferenceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(preferenceValue);

        var cleanedKey = preferenceKey.Trim();
        var cleanedValue = preferenceValue.Trim();

        if (cleanedKey.Length > 50)
            throw new ArgumentException("Preference key cannot exceed 50 characters.", nameof(preferenceKey));

        if (cleanedValue.Length > 250)
            throw new ArgumentException("Preference value cannot exceed 250 characters.", nameof(preferenceValue));

        var preference = new GuestPreference
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            GuestId = guestId,
            GuestUid = guestUid,
            PreferenceType = preferenceType,
            PreferenceKey = cleanedKey,
            PreferenceValue = cleanedValue,
            Notes = Optional(notes, 2000)
        };

        preference.MarkCreated(actorSubject);
        return preference;
    }

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.");

        return trimmed;
    }
}
