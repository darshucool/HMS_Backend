using HMS.Modules.Hotels.Domain.Common;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class AccommodationType : AuditableEntity
{
    private AccommodationType() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public Guid PropertyUid { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public UnitKind UnitKind { get; private set; } = UnitKind.Room;
    public string? Description { get; private set; }
    public int MaxAdults { get; private set; } = 1;
    public int MaxChildren { get; private set; }
    public int MaxOccupancy { get; private set; } = 1;
    public int DefaultQuantity { get; private set; } = 1;
    public decimal BaseRate { get; private set; }
    public int SortOrder { get; private set; }

    public static AccommodationType Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        string code,
        string name,
        UnitKind unitKind,
        string? description,
        int maxAdults,
        int maxChildren,
        int maxOccupancy,
        int defaultQuantity,
        decimal baseRate,
        int sortOrder,
        string actorSubject)
    {
        ValidateCapacity(maxAdults, maxChildren, maxOccupancy);
        ValidateQuantity(defaultQuantity);
        ValidateRate(baseRate);

        var accommodationType = new AccommodationType
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            Code = Required(code, nameof(code), 30).ToUpperInvariant(),
            Name = Required(name, nameof(name), 150),
            UnitKind = unitKind,
            Description = Clean(description),
            MaxAdults = maxAdults,
            MaxChildren = maxChildren,
            MaxOccupancy = maxOccupancy,
            DefaultQuantity = defaultQuantity,
            BaseRate = baseRate,
            SortOrder = sortOrder
        };

        accommodationType.MarkCreated(actorSubject);
        return accommodationType;
    }

    public void Update(
        string code,
        string name,
        UnitKind unitKind,
        string? description,
        int maxAdults,
        int maxChildren,
        int maxOccupancy,
        int defaultQuantity,
        decimal baseRate,
        int sortOrder,
        bool isActive,
        string actorSubject)
    {
        ValidateCapacity(maxAdults, maxChildren, maxOccupancy);
        ValidateQuantity(defaultQuantity);
        ValidateRate(baseRate);

        Code = Required(code, nameof(code), 30).ToUpperInvariant();
        Name = Required(name, nameof(name), 150);
        UnitKind = unitKind;
        Description = Clean(description);
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        MaxOccupancy = maxOccupancy;
        DefaultQuantity = defaultQuantity;
        BaseRate = baseRate;
        SortOrder = sortOrder;
        IsActive = isActive;
        MarkModified(actorSubject);
    }

    public void Archive(string actorSubject)
    {
        IsArchived = true;
        IsActive = false;
        MarkModified(actorSubject);
    }

    public static AccommodationType Rehydrate(
        long id,
        Guid uid,
        long organizationId,
        long propertyId,
        Guid propertyUid,
        string code,
        string name,
        UnitKind unitKind,
        string? description,
        int maxAdults,
        int maxChildren,
        int maxOccupancy,
        int defaultQuantity,
        decimal baseRate,
        int sortOrder,
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
            Code = code,
            Name = name,
            UnitKind = unitKind,
            Description = description,
            MaxAdults = maxAdults,
            MaxChildren = maxChildren,
            MaxOccupancy = maxOccupancy,
            DefaultQuantity = defaultQuantity,
            BaseRate = baseRate,
            SortOrder = sortOrder,
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

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateCapacity(int maxAdults, int maxChildren, int maxOccupancy)
    {
        if (maxAdults < 0)
            throw new ArgumentOutOfRangeException(nameof(maxAdults), "Max adults cannot be negative.");
        if (maxChildren < 0)
            throw new ArgumentOutOfRangeException(nameof(maxChildren), "Max children cannot be negative.");
        if (maxOccupancy <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxOccupancy), "Max occupancy must be greater than zero.");
        if (maxOccupancy < maxAdults)
            throw new ArgumentException("Max occupancy cannot be less than max adults.", nameof(maxOccupancy));
    }

    private static void ValidateQuantity(int defaultQuantity)
    {
        if (defaultQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(defaultQuantity), "Default quantity cannot be negative.");
    }

    private static void ValidateRate(decimal baseRate)
    {
        if (baseRate < 0)
            throw new ArgumentOutOfRangeException(nameof(baseRate), "Base rate cannot be negative.");
    }
}
