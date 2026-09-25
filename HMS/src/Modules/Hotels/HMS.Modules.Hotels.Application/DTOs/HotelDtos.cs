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

public sealed record MealPlanDto(
    Guid Uid,
    Guid PropertyUid,
    string Code,
    string Name,
    string? Description,
    bool IncludesBreakfast,
    bool IncludesLunch,
    bool IncludesDinner,
    bool AllowByo,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record RatePlanDto(
    Guid Uid,
    Guid PropertyUid,
    Guid AccommodationTypeUid,
    Guid? MealPlanUid,
    string Code,
    string Name,
    string PricingBasis,
    string Currency,
    string? Description,
    bool IsRefundable,
    bool IsActive,
    DateTimeOffset CreationDate);

public sealed record RatePlanPriceDto(
    Guid Uid,
    Guid RatePlanUid,
    Guid PropertyUid,
    DateOnly StartDate,
    DateOnly EndDate,
    short? DayOfWeek,
    decimal? AdultRate,
    decimal? ChildRate,
    decimal UnitRate,
    int MinimumStay,
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

public sealed record PropertyAvailabilityDto(
    Guid PropertyUid,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    int Adults,
    int Children,
    string Currency,
    IReadOnlyList<PropertyAvailabilityItemDto> Items);

public sealed record PropertyAvailabilityItemDto(
    Guid AccommodationTypeUid,
    string Code,
    string Name,
    string UnitKind,
    int MaxAdults,
    int MaxChildren,
    int MaxOccupancy,
    int AvailableUnits,
    int TotalUnits,
    decimal BaseRate,
    decimal EstimatedTotal);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount);

