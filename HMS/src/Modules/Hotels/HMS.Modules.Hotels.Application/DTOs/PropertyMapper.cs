using HMS.Modules.Hotels.Domain.Entities;

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
}

