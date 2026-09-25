using HMS.Modules.Hotels.Domain.Common;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class AccommodationUnit : AuditableEntity
{
    private AccommodationUnit() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public Guid PropertyUid { get; private set; }
    public long AccommodationTypeId { get; private set; }
    public Guid AccommodationTypeUid { get; private set; }
    public string UnitCode { get; private set; } = string.Empty;
    public string? UnitName { get; private set; }
    public string? FloorOrArea { get; private set; }
    public AccommodationUnitStatus Status { get; private set; } = AccommodationUnitStatus.Available;
    public HousekeepingStatus HousekeepingStatus { get; private set; } = HousekeepingStatus.Clean;
    public string? Notes { get; private set; }

    public static AccommodationUnit Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long accommodationTypeId,
        Guid accommodationTypeUid,
        string unitCode,
        string? unitName,
        string? floorOrArea,
        AccommodationUnitStatus status,
        HousekeepingStatus housekeepingStatus,
        string? notes,
        string actorSubject)
    {
        var unit = new AccommodationUnit
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            AccommodationTypeId = accommodationTypeId,
            AccommodationTypeUid = accommodationTypeUid,
            UnitCode = Required(unitCode, nameof(unitCode), 50),
            UnitName = Clean(unitName, 150),
            FloorOrArea = Clean(floorOrArea, 100),
            Status = status,
            HousekeepingStatus = housekeepingStatus,
            Notes = Clean(notes)
        };

        unit.MarkCreated(actorSubject);
        return unit;
    }

    public void Update(
        long accommodationTypeId,
        Guid accommodationTypeUid,
        string unitCode,
        string? unitName,
        string? floorOrArea,
        AccommodationUnitStatus status,
        HousekeepingStatus housekeepingStatus,
        string? notes,
        bool isActive,
        string actorSubject)
    {
        AccommodationTypeId = accommodationTypeId;
        AccommodationTypeUid = accommodationTypeUid;
        UnitCode = Required(unitCode, nameof(unitCode), 50);
        UnitName = Clean(unitName, 150);
        FloorOrArea = Clean(floorOrArea, 100);
        Status = status;
        HousekeepingStatus = housekeepingStatus;
        Notes = Clean(notes);
        IsActive = isActive;
        MarkModified(actorSubject);
    }

    public static AccommodationUnit Rehydrate(
        long id,
        Guid uid,
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long accommodationTypeId,
        Guid accommodationTypeUid,
        string unitCode,
        string? unitName,
        string? floorOrArea,
        AccommodationUnitStatus status,
        HousekeepingStatus housekeepingStatus,
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
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            AccommodationTypeId = accommodationTypeId,
            AccommodationTypeUid = accommodationTypeUid,
            UnitCode = unitCode,
            UnitName = unitName,
            FloorOrArea = floorOrArea,
            Status = status,
            HousekeepingStatus = housekeepingStatus,
            Notes = notes,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };

    private static string Required(string value, string name, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", name);

        return trimmed;
    }

    private static string? Clean(string? value, int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (maxLength is not null && trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.");

        return trimmed;
    }
}
