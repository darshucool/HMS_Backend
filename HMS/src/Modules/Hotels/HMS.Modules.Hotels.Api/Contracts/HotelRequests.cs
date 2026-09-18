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

public sealed record UpdatePropertySettingsRequest(
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    string BookingNumberPrefix,
    string InvoiceNumberPrefix,
    decimal TaxRate,
    decimal ServiceChargeRate,
    bool AllowOverbooking,
    JsonElement? ExtraSettings);

