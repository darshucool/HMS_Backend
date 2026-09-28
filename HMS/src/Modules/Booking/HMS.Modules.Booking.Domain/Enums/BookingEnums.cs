namespace HMS.Modules.Booking.Domain.Enums;

public enum BookingSource
{
    WalkIn,
    Phone,
    WhatsApp,
    Direct,
    Website,
    TravelAgent,
    BookingCom,
    Airbnb,
    Other
}

public static class BookingSourceMapper
{
    public static string ToDatabaseValue(this BookingSource source) => source switch
    {
        BookingSource.WalkIn => "WALK_IN",
        BookingSource.Phone => "PHONE",
        BookingSource.WhatsApp => "WHATSAPP",
        BookingSource.Direct => "DIRECT",
        BookingSource.Website => "WEBSITE",
        BookingSource.TravelAgent => "TRAVEL_AGENT",
        BookingSource.BookingCom => "BOOKING_COM",
        BookingSource.Airbnb => "AIRBNB",
        BookingSource.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unsupported booking source.")
    };

    public static BookingSource FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "WALK_IN" => BookingSource.WalkIn,
            "PHONE" => BookingSource.Phone,
            "WHATSAPP" => BookingSource.WhatsApp,
            "DIRECT" => BookingSource.Direct,
            "WEBSITE" => BookingSource.Website,
            "TRAVEL_AGENT" or "TRAVELAGENT" => BookingSource.TravelAgent,
            "BOOKING_COM" or "BOOKINGCOM" => BookingSource.BookingCom,
            "AIRBNB" => BookingSource.Airbnb,
            "OTHER" => BookingSource.Other,
            _ => throw new ArgumentException($"Unsupported booking source '{value}'.", nameof(value))
        };
    }
}

public enum PricingBasis
{
    PerRoomPerNight,
    PerPersonPerNight,
    PerBedPerNight,
    FlatPerStay
}

public static class PricingBasisMapper
{
    public static string ToDatabaseValue(this PricingBasis basis) => basis switch
    {
        PricingBasis.PerRoomPerNight => "PER_ROOM_PER_NIGHT",
        PricingBasis.PerPersonPerNight => "PER_PERSON_PER_NIGHT",
        PricingBasis.PerBedPerNight => "PER_BED_PER_NIGHT",
        PricingBasis.FlatPerStay => "FLAT_PER_STAY",
        _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "Unsupported pricing basis.")
    };
}

public enum BookingStatus
{
    Inquiry,
    Pending,
    Tentative,
    Confirmed,
    CheckedIn,
    CheckedOut,
    Completed,
    Cancelled,
    NoShow
}

public static class BookingStatusMapper
{
    public static string ToDatabaseValue(this BookingStatus status) => status switch
    {
        BookingStatus.Inquiry => "INQUIRY",
        BookingStatus.Pending => "PENDING",
        BookingStatus.Tentative => "TENTATIVE",
        BookingStatus.Confirmed => "CONFIRMED",
        BookingStatus.CheckedIn => "CHECKED_IN",
        BookingStatus.CheckedOut => "CHECKED_OUT",
        BookingStatus.Completed => "COMPLETED",
        BookingStatus.Cancelled => "CANCELLED",
        BookingStatus.NoShow => "NO_SHOW",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported booking status.")
    };

    public static BookingStatus FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "INQUIRY" => BookingStatus.Inquiry,
            "PENDING" => BookingStatus.Pending,
            "TENTATIVE" => BookingStatus.Tentative,
            "CONFIRMED" => BookingStatus.Confirmed,
            "CHECKED_IN" => BookingStatus.CheckedIn,
            "CHECKED_OUT" => BookingStatus.CheckedOut,
            "COMPLETED" => BookingStatus.Completed,
            "CANCELLED" => BookingStatus.Cancelled,
            "NO_SHOW" or "NOSHOW" => BookingStatus.NoShow,
            _ => throw new ArgumentException($"Unsupported booking status '{value}'.", nameof(value))
        };
    }
}
