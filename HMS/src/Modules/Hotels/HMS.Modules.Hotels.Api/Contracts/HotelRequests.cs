using System.Text.Json;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Api.Contracts;

public sealed record CreateOrganizationRequest(
    string Code,
    string Name,
    string? LegalName = null,
    string DefaultCurrency = "LKR",
    string Timezone = "Asia/Colombo");

public sealed record CreatePropertyRequest(
    string Code,
    string Name,
    string Slug,
    PropertyType PropertyType,
    string? Description = null,
    string? AddressLine1 = null,
    string? AddressLine2 = null,
    string? City = null,
    string? District = null,
    string? Province = null,
    string? PostalCode = null,
    string CountryCode = "LK",
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? Phone = null,
    string? Email = null,
    string Timezone = "Asia/Colombo",
    string DefaultCurrency = "LKR");

public sealed record UpdatePropertyRequest(
    string Name,
    PropertyType PropertyType,
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
    PropertyStatus Status);

public sealed record CreateAccommodationTypeRequest(
    string Code,
    string Name,
    UnitKind UnitKind = UnitKind.Room,
    string? Description = null,
    int MaxAdults = 1,
    int MaxChildren = 0,
    int MaxOccupancy = 1,
    int DefaultQuantity = 1,
    decimal BaseRate = 0,
    int SortOrder = 0);

public sealed record CreateMealPlanRequest(
    string Code,
    string Name,
    string? Description = null,
    bool IncludesBreakfast = false,
    bool IncludesLunch = false,
    bool IncludesDinner = false,
    bool AllowByo = false);

public sealed record CreateRatePlanRequest(
    Guid AccommodationTypeUid,
    string Code,
    string Name,
    PricingBasis PricingBasis,
    Guid? MealPlanUid = null,
    string Currency = "LKR",
    string? Description = null,
    bool IsRefundable = true);

public sealed record CreateRatePlanPriceRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    decimal UnitRate,
    short? DayOfWeek = null,
    decimal? AdultRate = null,
    decimal? ChildRate = null,
    int MinimumStay = 1);

public sealed record UpdateAccommodationTypeRequest(
    string Code,
    string Name,
    UnitKind UnitKind,
    string? Description,
    int MaxAdults,
    int MaxChildren,
    int MaxOccupancy,
    int DefaultQuantity,
    decimal BaseRate,
    int SortOrder,
    bool IsActive);

public sealed record CreateAccommodationUnitRequest(
    Guid AccommodationTypeUid,
    string UnitCode,
    string? UnitName = null,
    string? FloorOrArea = null,
    AccommodationUnitStatus Status = AccommodationUnitStatus.Available,
    HousekeepingStatus HousekeepingStatus = HousekeepingStatus.Clean,
    string? Notes = null);

public sealed record UpdateAccommodationUnitRequest(
    Guid AccommodationTypeUid,
    string UnitCode,
    string? UnitName,
    string? FloorOrArea,
    AccommodationUnitStatus Status,
    HousekeepingStatus HousekeepingStatus,
    string? Notes,
    bool IsActive);

public sealed record CreateUnitBlockRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    UnitBlockType BlockType,
    string? Reason = null);

public sealed record UpdatePropertySettingsRequest(
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    string BookingNumberPrefix,
    string InvoiceNumberPrefix,
    decimal TaxRate,
    decimal ServiceChargeRate,
    bool AllowOverbooking,
    JsonElement? ExtraSettings);

