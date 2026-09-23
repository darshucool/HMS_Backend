using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Application.DTOs;

internal static class GuestMapper
{
    public static GuestDto ToDto(Guest item) => new(
        item.Uid,
        item.OrganizationUid,
        item.GuestType.ToDatabaseValue(),
        item.Title,
        item.FirstName,
        item.LastName,
        item.DisplayName,
        item.Phone,
        item.AlternatePhone,
        item.Email,
        item.NationalityCode,
        item.DateOfBirth,
        item.PreferredLanguage,
        item.Address,
        item.City,
        item.CountryCode,
        item.Notes,
        item.IsActive,
        item.CreationDate);
}
