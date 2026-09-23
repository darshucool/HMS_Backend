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
