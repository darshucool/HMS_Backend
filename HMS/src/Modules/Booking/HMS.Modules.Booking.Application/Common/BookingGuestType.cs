namespace HMS.Modules.Booking.Application.Common;

public static class BookingGuestType
{
    public static bool TryParse(string? value, out string databaseValue)
    {
        databaseValue = Normalize(value) switch
        {
            "SINGLE" or "INDIVIDUAL" => "INDIVIDUAL",
            "COUPLE" => "COUPLE",
            "FAMILY" => "FAMILY",
            "GROUP" => "GROUP",
            "CORPORATE" => "CORPORATE",
            "TRAVEL_AGENT" or "TRAVELAGENT" => "TRAVEL_AGENT",
            _ => string.Empty
        };

        return databaseValue.Length > 0;
    }

    public static string? ToDisplay(string? databaseValue) => Normalize(databaseValue) switch
    {
        "" => null,
        "INDIVIDUAL" or "SINGLE" => "Single",
        "COUPLE" => "Couple",
        "FAMILY" => "Family",
        "GROUP" => "Group",
        "CORPORATE" => "Corporate",
        "TRAVEL_AGENT" => "Travel Agent",
        _ => null
    };

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant().Replace(' ', '_').Replace('-', '_');
}
