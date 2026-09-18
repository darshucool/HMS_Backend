namespace HMS.Modules.Hotels.Domain.Enums;

public enum OrganizationStatus
{
    Trial,
    Active,
    Suspended,
    Closed
}

public enum PropertyStatus
{
    Draft,
    Active,
    Suspended,
    Closed
}

public enum PropertyType
{
    Hotel,
    Guesthouse,
    Villa,
    Apartment,
    Hostel,
    Homestay,
    Resort,
    Campsite,
    Lodge,
    Bungalow,
    Other
}

public enum UnitKind
{
    Room,
    Villa,
    Apartment,
    Cabin,
    Tent,
    DormBed,
    Cottage,
    EntireProperty,
    Other
}

public static class UnitKindMapper
{
    public static string ToDatabaseValue(this UnitKind kind) => kind switch
    {
        UnitKind.Room => "ROOM",
        UnitKind.Villa => "VILLA",
        UnitKind.Apartment => "APARTMENT",
        UnitKind.Cabin => "CABIN",
        UnitKind.Tent => "TENT",
        UnitKind.DormBed => "DORM_BED",
        UnitKind.Cottage => "COTTAGE",
        UnitKind.EntireProperty => "ENTIRE_PROPERTY",
        UnitKind.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported unit kind.")
    };

    public static UnitKind FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "ROOM" => UnitKind.Room,
            "VILLA" => UnitKind.Villa,
            "APARTMENT" => UnitKind.Apartment,
            "CABIN" => UnitKind.Cabin,
            "TENT" => UnitKind.Tent,
            "DORM_BED" or "DORMBED" => UnitKind.DormBed,
            "COTTAGE" => UnitKind.Cottage,
            "ENTIRE_PROPERTY" or "ENTIREPROPERTY" => UnitKind.EntireProperty,
            "OTHER" => UnitKind.Other,
            _ => throw new ArgumentException($"Unsupported unit kind '{value}'.", nameof(value))
        };
    }
}

