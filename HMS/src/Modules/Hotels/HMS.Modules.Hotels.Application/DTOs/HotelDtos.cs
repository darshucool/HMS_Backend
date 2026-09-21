namespace HMS.Modules.Hotels.Application.DTOs;

public sealed record OrganizationDto(
    Guid Uid,
    string Code,
    string Name,
    string? LegalName,
    string DefaultCurrency,
    string Timezone,
    string Status,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record PropertyDto(
    Guid Uid,
    Guid OrganizationUid,
    string Code,
    string Name,
    string Slug,
    string PropertyType,
    string? Description,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? District,
    string? Province,
    string? PostalCode,
    string CountryCode,
    decimal? Latitude,
    decimal? Longitude,
    string? Phone,
    string? Email,
    string Timezone,
    string DefaultCurrency,
    string Status,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record PropertySettingsDto(
    Guid PropertyUid,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    string BookingNumberPrefix,
    string InvoiceNumberPrefix,
    decimal TaxRate,
    decimal ServiceChargeRate,
    bool AllowOverbooking,
    string ExtraSettingsJson);

public sealed record MyPropertyDto(
    Guid Uid,
    string Code,
    string Name,
    string Slug,
    string PropertyType,
    string RoleCode,
    bool IsDefaultProperty,
    string Status);

public sealed record AccommodationTypeDto(
    Guid Uid,
    Guid PropertyUid,
    string Code,
    string Name,
    string UnitKind,
    string? Description,
    int MaxAdults,
    int MaxChildren,
    int MaxOccupancy,
    int DefaultQuantity,
    decimal BaseRate,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record AccommodationUnitDto(
    Guid Uid,
    Guid PropertyUid,
    Guid AccommodationTypeUid,
    string UnitCode,
    string? UnitName,
    string? FloorOrArea,
    string Status,
    string HousekeepingStatus,
    string? Notes,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record UnitBlockDto(
    Guid Uid,
    Guid UnitUid,
    Guid PropertyUid,
    DateOnly StartDate,
    DateOnly EndDate,
    string BlockType,
    string? Reason,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);

