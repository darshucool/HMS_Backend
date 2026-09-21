using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Application.DTOs;

internal static class PropertyMapper
{
    public static PropertyDto ToDto(Property item) => new(
        item.Uid,
        item.OrganizationUid,
        item.Code,
        item.Name,
        item.Slug,
        item.PropertyType.ToString().ToUpperInvariant(),
        item.Description,
        item.AddressLine1,
        item.AddressLine2,
        item.City,
        item.District,
        item.Province,
        item.PostalCode,
        item.CountryCode,
        item.Latitude,
        item.Longitude,
        item.Phone,
        item.Email,
        item.Timezone,
        item.DefaultCurrency,
        item.Status.ToString().ToUpperInvariant(),
        item.IsActive,
        item.CreationDate);

    public static PropertySettingsDto ToSettingsDto(Guid propertyUid, PropertySettings settings) => new(
        propertyUid,
        settings.CheckInTime,
        settings.CheckOutTime,
        settings.BookingNumberPrefix,
        settings.InvoiceNumberPrefix,
        settings.TaxRate,
        settings.ServiceChargeRate,
        settings.AllowOverbooking,
        settings.ExtraSettingsJson);

    public static AccommodationTypeDto ToAccommodationTypeDto(AccommodationType item) => new(
        item.Uid,
        item.PropertyUid,
        item.Code,
        item.Name,
        item.UnitKind.ToDatabaseValue(),
        item.Description,
        item.MaxAdults,
        item.MaxChildren,
        item.MaxOccupancy,
        item.DefaultQuantity,
        item.BaseRate,
        item.SortOrder,
        item.IsActive,
        item.CreationDate);

    public static AccommodationUnitDto ToAccommodationUnitDto(AccommodationUnit item) => new(
        item.Uid,
        item.PropertyUid,
        item.AccommodationTypeUid,
        item.UnitCode,
        item.UnitName,
        item.FloorOrArea,
        item.Status.ToDatabaseValue(),
        item.HousekeepingStatus.ToDatabaseValue(),
        item.Notes,
        item.IsActive,
        item.CreationDate);

    public static UnitBlockDto ToUnitBlockDto(UnitBlock item) => new(
        item.Uid,
        item.UnitUid,
        item.PropertyUid,
        item.StartDate,
        item.EndDate,
        item.BlockType.ToDatabaseValue(),
        item.Reason,
        item.IsActive,
        item.CreationDate);
}

