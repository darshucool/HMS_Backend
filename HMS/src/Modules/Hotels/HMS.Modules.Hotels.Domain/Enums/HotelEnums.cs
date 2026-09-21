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

public enum AccommodationUnitStatus
{
    Available,
    Occupied,
    OutOfService,
    Maintenance,
    Inactive
}

public static class AccommodationUnitStatusMapper
{
    public static string ToDatabaseValue(this AccommodationUnitStatus status) => status switch
    {
        AccommodationUnitStatus.Available => "AVAILABLE",
        AccommodationUnitStatus.Occupied => "OCCUPIED",
        AccommodationUnitStatus.OutOfService => "OUT_OF_SERVICE",
        AccommodationUnitStatus.Maintenance => "MAINTENANCE",
        AccommodationUnitStatus.Inactive => "INACTIVE",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported unit status.")
    };

    public static AccommodationUnitStatus FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "AVAILABLE" => AccommodationUnitStatus.Available,
            "OCCUPIED" => AccommodationUnitStatus.Occupied,
            "OUT_OF_SERVICE" or "OUTOFSERVICE" => AccommodationUnitStatus.OutOfService,
            "MAINTENANCE" => AccommodationUnitStatus.Maintenance,
            "INACTIVE" => AccommodationUnitStatus.Inactive,
            _ => throw new ArgumentException($"Unsupported unit status '{value}'.", nameof(value))
        };
    }
}

public enum HousekeepingStatus
{
    Clean,
    Dirty,
    Inspected,
    InProgress,
    NotApplicable
}

public static class HousekeepingStatusMapper
{
    public static string ToDatabaseValue(this HousekeepingStatus status) => status switch
    {
        HousekeepingStatus.Clean => "CLEAN",
        HousekeepingStatus.Dirty => "DIRTY",
        HousekeepingStatus.Inspected => "INSPECTED",
        HousekeepingStatus.InProgress => "IN_PROGRESS",
        HousekeepingStatus.NotApplicable => "NOT_APPLICABLE",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported housekeeping status.")
    };

    public static HousekeepingStatus FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "CLEAN" => HousekeepingStatus.Clean,
            "DIRTY" => HousekeepingStatus.Dirty,
            "INSPECTED" => HousekeepingStatus.Inspected,
            "IN_PROGRESS" or "INPROGRESS" => HousekeepingStatus.InProgress,
            "NOT_APPLICABLE" or "NOTAPPLICABLE" => HousekeepingStatus.NotApplicable,
            _ => throw new ArgumentException($"Unsupported housekeeping status '{value}'.", nameof(value))
        };
    }
}

public enum UnitBlockType
{
    Maintenance,
    OwnerUse,
    Closed,
    Other
}

public static class UnitBlockTypeMapper
{
    public static string ToDatabaseValue(this UnitBlockType type) => type switch
    {
        UnitBlockType.Maintenance => "MAINTENANCE",
        UnitBlockType.OwnerUse => "OWNER_USE",
        UnitBlockType.Closed => "CLOSED",
        UnitBlockType.Other => "OTHER",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported block type.")
    };

    public static UnitBlockType FromDatabaseValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant().Replace("-", "_") switch
        {
            "MAINTENANCE" => UnitBlockType.Maintenance,
            "OWNER_USE" or "OWNERUSE" => UnitBlockType.OwnerUse,
            "CLOSED" => UnitBlockType.Closed,
            "OTHER" => UnitBlockType.Other,
            _ => throw new ArgumentException($"Unsupported block type '{value}'.", nameof(value))
        };
    }
}

