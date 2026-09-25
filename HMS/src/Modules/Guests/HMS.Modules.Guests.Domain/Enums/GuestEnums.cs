namespace HMS.Modules.Guests.Domain.Enums;

public enum GuestType
{
    Individual,
    Couple,
    Family,
    Group,
    Corporate,
    TravelAgent
}

public static class GuestTypeMapper
{
    public static string ToDatabaseValue(this GuestType type) => type switch
    {
        GuestType.Individual => "INDIVIDUAL",
        GuestType.Couple => "COUPLE",
        GuestType.Family => "FAMILY",
        GuestType.Group => "GROUP",
        GuestType.Corporate => "CORPORATE",
        GuestType.TravelAgent => "TRAVEL_AGENT",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported guest type.")
    };

    public static GuestType FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "INDIVIDUAL" => GuestType.Individual,
            "COUPLE" => GuestType.Couple,
            "FAMILY" => GuestType.Family,
            "GROUP" => GuestType.Group,
            "CORPORATE" => GuestType.Corporate,
            "TRAVEL_AGENT" or "TRAVELAGENT" => GuestType.TravelAgent,
            _ => throw new ArgumentException($"Unsupported guest type '{value}'.", nameof(value))
        };
    }
}

public enum DocumentType
{
    Nic,
    Passport,
    DrivingLicence,
    Other
}

public static class DocumentTypeMapper
{
    public static string ToDatabaseValue(this DocumentType type) => type switch
    {
        DocumentType.Nic => "NIC",
        DocumentType.Passport => "PASSPORT",
        DocumentType.DrivingLicence => "DRIVING_LICENCE",
        DocumentType.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported document type.")
    };

    public static DocumentType FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "NIC" => DocumentType.Nic,
            "PASSPORT" => DocumentType.Passport,
            "DRIVING_LICENCE" or "DRIVINGLICENCE" => DocumentType.DrivingLicence,
            "OTHER" => DocumentType.Other,
            _ => throw new ArgumentException($"Unsupported document type '{value}'.", nameof(value))
        };
    }
}

public enum PreferenceType
{
    Room,
    Dietary,
    Accessibility,
    Communication,
    Other
}

public static class PreferenceTypeMapper
{
    public static string ToDatabaseValue(this PreferenceType type) => type switch
    {
        PreferenceType.Room => "ROOM",
        PreferenceType.Dietary => "DIETARY",
        PreferenceType.Accessibility => "ACCESSIBILITY",
        PreferenceType.Communication => "COMMUNICATION",
        PreferenceType.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported preference type.")
    };

    public static PreferenceType FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "ROOM" => PreferenceType.Room,
            "DIETARY" => PreferenceType.Dietary,
            "ACCESSIBILITY" => PreferenceType.Accessibility,
            "COMMUNICATION" => PreferenceType.Communication,
            "OTHER" => PreferenceType.Other,
            _ => throw new ArgumentException($"Unsupported preference type '{value}'.", nameof(value))
        };
    }
}
