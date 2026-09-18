using HMS.Modules.Hotels.Domain.Common;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class Property : AuditableEntity
{
    private Property() { }

    public long OrganizationId { get; private set; }
    public Guid OrganizationUid { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public PropertyType PropertyType { get; private set; }
    public string? Description { get; private set; }
    public string? AddressLine1 { get; private set; }
    public string? AddressLine2 { get; private set; }
    public string? City { get; private set; }
    public string? District { get; private set; }
    public string? Province { get; private set; }
    public string? PostalCode { get; private set; }
    public string CountryCode { get; private set; } = "LK";
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string Timezone { get; private set; } = "Asia/Colombo";
    public string DefaultCurrency { get; private set; } = "LKR";
    public PropertyStatus Status { get; private set; } = PropertyStatus.Active;

    public static Property Create(
        long organizationId,
        Guid organizationUid,
        string code,
        string name,
        string slug,
        PropertyType propertyType,
        string? description,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? district,
        string? province,
        string? postalCode,
        string countryCode,
        decimal? latitude,
        decimal? longitude,
        string? phone,
        string? email,
        string timezone,
        string defaultCurrency,
        string actorSubject)
    {
        ValidateCoordinates(latitude, longitude);

        var property = new Property
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            OrganizationUid = organizationUid,
            Code = Required(code, nameof(code)).ToUpperInvariant(),
            Name = Required(name, nameof(name)),
            Slug = Required(slug, nameof(slug)).ToLowerInvariant(),
            PropertyType = propertyType,
            Description = Clean(description),
            AddressLine1 = Clean(addressLine1),
            AddressLine2 = Clean(addressLine2),
            City = Clean(city),
            District = Clean(district),
            Province = Clean(province),
            PostalCode = Clean(postalCode),
            CountryCode = Required(countryCode, nameof(countryCode)).ToUpperInvariant(),
            Latitude = latitude,
            Longitude = longitude,
            Phone = Clean(phone),
            Email = Clean(email),
            Timezone = Required(timezone, nameof(timezone)),
            DefaultCurrency = Required(defaultCurrency, nameof(defaultCurrency)).ToUpperInvariant(),
            Status = PropertyStatus.Active
        };

        property.MarkCreated(actorSubject);
        return property;
    }

    public void Update(
        string name,
        PropertyType propertyType,
        string? description,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? district,
        string? province,
        string? postalCode,
        string countryCode,
        decimal? latitude,
        decimal? longitude,
        string? phone,
        string? email,
        string timezone,
        string defaultCurrency,
        PropertyStatus status,
        string actorSubject)
    {
        ValidateCoordinates(latitude, longitude);
        Name = Required(name, nameof(name));
        PropertyType = propertyType;
        Description = Clean(description);
        AddressLine1 = Clean(addressLine1);
        AddressLine2 = Clean(addressLine2);
        City = Clean(city);
        District = Clean(district);
        Province = Clean(province);
        PostalCode = Clean(postalCode);
        CountryCode = Required(countryCode, nameof(countryCode)).ToUpperInvariant();
        Latitude = latitude;
        Longitude = longitude;
        Phone = Clean(phone);
        Email = Clean(email);
        Timezone = Required(timezone, nameof(timezone));
        DefaultCurrency = Required(defaultCurrency, nameof(defaultCurrency)).ToUpperInvariant();
        Status = status;
        IsActive = status == PropertyStatus.Active;
        MarkModified(actorSubject);
    }

    public static Property Rehydrate(
        long id, Guid uid, long organizationId, Guid organizationUid,
        string code, string name, string slug, PropertyType propertyType,
        string? description, string? addressLine1, string? addressLine2,
        string? city, string? district, string? province, string? postalCode,
        string countryCode, decimal? latitude, decimal? longitude,
        string? phone, string? email, string timezone, string defaultCurrency,
        PropertyStatus status, bool isActive, bool isArchived,
        DateTimeOffset creationDate, string? createdBy,
        DateTimeOffset? modifiedDate, string? modifiedBy) =>
        new()
        {
            Id = id,
            Uid = uid,
            OrganizationId = organizationId,
            OrganizationUid = organizationUid,
            Code = code,
            Name = name,
            Slug = slug,
            PropertyType = propertyType,
            Description = description,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            District = district,
            Province = province,
            PostalCode = postalCode,
            CountryCode = countryCode,
            Latitude = latitude,
            Longitude = longitude,
            Phone = phone,
            Email = email,
            Timezone = timezone,
            DefaultCurrency = defaultCurrency,
            Status = status,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };

    private static string Required(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        return value.Trim();
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateCoordinates(decimal? latitude, decimal? longitude)
    {
        if (latitude is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude));
    }
}

