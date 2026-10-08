using System.Text.Json;
using System.Text.Json.Serialization;
using HMS.Modules.Booking.Domain.Enums;

namespace HMS.Modules.Booking.Api.Contracts;

public sealed record CreateBookingUnitRequest(
    Guid AccommodationTypeUid,
    Guid RatePlanUid,
    Guid? UnitUid = null,
    int Adults = 1,
    int Children = 0,
    int UnitQuantity = 1,
    int GuestCount = 1,
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    string? Notes = null);

public sealed record CreateBookingRequest(
    Guid LeadGuestUid,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    BookingSource BookingSource = BookingSource.Direct,
    int Adults = 1,
    int Children = 0,
    int Infants = 0,
    string Currency = "LKR",
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    decimal ServiceCharge = 0,
    TimeOnly? ArrivalTime = null,
    TimeOnly? DepartureTime = null,
    string? SpecialRequests = null,
    string? InternalNotes = null,
    string? ExternalReference = null,
    string? GuestType = null,
    decimal CookingCharges = 0,
    decimal ExtraCharges = 0,
    IReadOnlyList<CreateBookingUnitRequest>? Units = null)
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; init; }

    public decimal ResolveCookingCharges() =>
        CookingCharges != 0
            ? CookingCharges
            : ReadDecimal("cooking_charges", "cookingCharge", "cooking_charge");

    public decimal ResolveExtraCharges() =>
        ExtraCharges != 0
            ? ExtraCharges
            : ReadDecimal("extra_charges", "extraCharge", "extra_charge");

    private decimal ReadDecimal(params string[] names)
    {
        var text = ReadString(names);
        return decimal.TryParse(text, out var amount) ? amount : 0;
    }

    private string? ReadString(params string[] names)
    {
        if (AdditionalData is null)
            return null;

        foreach (var name in names)
        {
            var match = AdditionalData.FirstOrDefault(item =>
                string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(match.Key))
                continue;

            var value = match.Value.ValueKind switch
            {
                JsonValueKind.String => match.Value.GetString(),
                JsonValueKind.Number => match.Value.GetRawText(),
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }
}

public sealed record UpdateBookingRequest(
    Guid LeadGuestUid,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    BookingSource BookingSource = BookingSource.Direct,
    BookingStatus Status = BookingStatus.Pending,
    int Adults = 1,
    int Children = 0,
    int Infants = 0,
    string Currency = "LKR",
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    decimal ServiceCharge = 0,
    decimal? QuotedTotal = null,
    TimeOnly? ArrivalTime = null,
    TimeOnly? DepartureTime = null,
    string? SpecialRequests = null,
    string? InternalNotes = null,
    string? ExternalReference = null,
    string? CancellationReason = null,
    string? GuestType = null,
    decimal? CookingCharges = null,
    decimal? ExtraCharges = null);

public sealed record CancelBookingRequest(string Reason);

public sealed record AssignBookingUnitRequest(
    Guid BookingUnitUid,
    Guid UnitUid);

public sealed record AddBookingGuestRequest(
    Guid GuestUid,
    Guid? BookingUnitUid = null,
    bool IsLeadGuest = false);
