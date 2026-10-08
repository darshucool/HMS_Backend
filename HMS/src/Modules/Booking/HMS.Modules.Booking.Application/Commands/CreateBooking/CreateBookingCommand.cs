using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.Common;
using HMS.Modules.Booking.Application.DTOs;
using HMS.Modules.Booking.Domain.Enums;
using MediatR;
using HotelBooking = HMS.Modules.Booking.Domain.Entities.Booking;

namespace HMS.Modules.Booking.Application.Commands.CreateBooking;

public sealed record CreateBookingUnit(
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

public sealed record CreateBookingCommand(
    Guid PropertyUid,
    Guid LeadGuestUid,
    BookingSource BookingSource,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    int Infants,
    string Currency,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    TimeOnly? ArrivalTime,
    TimeOnly? DepartureTime,
    string? SpecialRequests,
    string? InternalNotes,
    string? ExternalReference,
    string? GuestType,
    decimal CookingCharges,
    decimal ExtraCharges,
    IReadOnlyList<CreateBookingUnit> Units,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<BookingResult<BookingDto>>;

public sealed class CreateBookingCommandHandler(
    IPropertyBookingAccess propertyAccess,
    IBookingRepository bookingRepository)
    : IRequestHandler<CreateBookingCommand, BookingResult<BookingDto>>
{
    public async Task<BookingResult<BookingDto>> Handle(
        CreateBookingCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return BookingResult<BookingDto>.Forbidden("You cannot create bookings for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return BookingResult<BookingDto>.NotFound("Property was not found.");

        var guest = await bookingRepository.GetGuestAsync(request.LeadGuestUid, cancellationToken);
        if (guest is null)
            return BookingResult<BookingDto>.NotFound("Lead guest was not found.");

        if (guest.OrganizationId != property.OrganizationId)
            return BookingResult<BookingDto>.Validation("Lead guest does not belong to this property's organization.");

        if (!string.IsNullOrWhiteSpace(request.GuestType) &&
            !BookingGuestType.TryParse(request.GuestType, out _))
        {
            return BookingResult<BookingDto>.Validation(
                "Guest type must be Single, Couple, Family, Group, Corporate, or Travel Agent.");
        }

        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
        if (nights <= 0)
            return BookingResult<BookingDto>.Validation("Check-out date must be after check-in date.");

        if (request.Units.Count == 0)
            return BookingResult<BookingDto>.Validation("At least one booking unit is required.");

        var lines = new List<BookingUnitLine>();
        decimal unitsTotal = 0;

        foreach (var unit in request.Units)
        {
            if (unit.RatePlanUid == Guid.Empty)
                return BookingResult<BookingDto>.Validation("Each booking unit requires a ratePlanUid.");

            if (unit.UnitQuantity <= 0 || unit.GuestCount <= 0 || unit.Adults < 0 || unit.Children < 0)
                return BookingResult<BookingDto>.Validation("Each booking unit needs a positive quantity and guest count.");

            if (unit.DiscountAmount < 0 || unit.TaxAmount < 0)
                return BookingResult<BookingDto>.Validation("Booking unit amounts cannot be negative.");

            var accommodationType = await bookingRepository.GetAccommodationTypeAsync(
                unit.AccommodationTypeUid,
                property.Id,
                cancellationToken);

            if (accommodationType is null)
                return BookingResult<BookingDto>.NotFound("Accommodation type was not found for this property.");

            long? unitId = null;
            if (unit.UnitUid is Guid unitUid)
            {
                unitId = await bookingRepository.GetUnitIdAsync(
                    unitUid,
                    property.Id,
                    accommodationType.Id,
                    cancellationToken);

                if (unitId is null)
                    return BookingResult<BookingDto>.NotFound("Accommodation unit was not found for this type.");
            }

            var ratePlan = await bookingRepository.GetRatePlanAsync(
                unit.RatePlanUid,
                property.Id,
                accommodationType.Id,
                cancellationToken);

            if (ratePlan is null)
            {
                return BookingResult<BookingDto>.Validation(
                    "Rate plan was not found for this property and accommodation type.");
            }

            PricingBasis pricingBasis;
            try
            {
                pricingBasis = PricingBasisMapper.FromDatabaseValue(ratePlan.PricingBasis);
            }
            catch (ArgumentException exception)
            {
                return BookingResult<BookingDto>.Validation(exception.Message);
            }

            var prices = await bookingRepository.GetRatePlanPricesAsync(
                ratePlan.Id,
                request.CheckInDate,
                request.CheckOutDate,
                cancellationToken);

            var priced = TryPriceStay(
                pricingBasis,
                prices,
                request.CheckInDate,
                request.CheckOutDate,
                nights,
                unit.UnitQuantity,
                unit.Adults,
                unit.Children,
                unit.GuestCount,
                out var unitRate,
                out var stayCharge,
                out var priceError);

            if (!priced)
                return BookingResult<BookingDto>.Validation(priceError!);

            var total = stayCharge - unit.DiscountAmount + unit.TaxAmount;
            if (total < 0)
                return BookingResult<BookingDto>.Validation("Booking unit total cannot be negative.");

            unitsTotal += total;
            lines.Add(new BookingUnitLine(
                accommodationType.Id,
                unitId,
                ratePlan.Id,
                request.CheckInDate,
                request.CheckOutDate,
                unit.Adults,
                unit.Children,
                unit.UnitQuantity,
                unit.GuestCount,
                pricingBasis.ToDatabaseValue(),
                unitRate,
                unit.DiscountAmount,
                unit.TaxAmount,
                total,
                unit.Notes));
        }

        var quotedTotal = unitsTotal
            + request.CookingCharges
            + request.ExtraCharges
            - request.DiscountAmount
            + request.TaxAmount
            + request.ServiceCharge;

        var bookingNumber = await bookingRepository.NextBookingNumberAsync(
            property.Id,
            property.BookingNumberPrefix,
            cancellationToken);

        HotelBooking booking;
        try
        {
            booking = HotelBooking.Create(
                property.OrganizationId,
                property.Id,
                request.PropertyUid,
                bookingNumber,
                guest.Id,
                request.LeadGuestUid,
                request.BookingSource,
                request.ExternalReference,
                request.CheckInDate,
                request.CheckOutDate,
                request.Adults,
                request.Children,
                request.Infants,
                request.Currency,
                request.DiscountAmount,
                request.TaxAmount,
                request.ServiceCharge,
                request.CookingCharges,
                request.ExtraCharges,
                quotedTotal,
                request.ArrivalTime,
                request.DepartureTime,
                request.SpecialRequests,
                request.InternalNotes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return BookingResult<BookingDto>.Validation(exception.Message);
        }

        try
        {
            await bookingRepository.InsertAsync(booking, lines, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return BookingResult<BookingDto>.Conflict(exception.Message);
        }

        if (!string.IsNullOrWhiteSpace(request.GuestType) &&
            BookingGuestType.TryParse(request.GuestType, out var guestType))
        {
            await bookingRepository.UpdateGuestTypeAsync(
                guest.Id,
                guestType,
                request.ActorSubject,
                cancellationToken);
        }

        var created = await bookingRepository.GetDetailByUidAsync(booking.Uid, cancellationToken);
        if (created is null)
            return BookingResult<BookingDto>.NotFound("Booking was not found.");

        return BookingResult<BookingDto>.Success(new BookingDto(
            created.Uid,
            created.PropertyUid,
            created.BookingNumber,
            created.LeadGuestUid,
            created.LeadGuestName,
            created.GuestType,
            created.CookingCharges,
            created.ExtraCharges,
            created.BookingSource,
            created.Status,
            created.CheckInDate,
            created.CheckOutDate,
            created.Nights,
            created.Adults,
            created.Children,
            created.Infants,
            created.Currency,
            created.QuotedTotal,
            created.SpecialRequests,
            created.CreationDate,
            created.Summary));
    }

    private static bool TryPriceStay(
        PricingBasis pricingBasis,
        IReadOnlyList<RatePlanPriceRow> prices,
        DateOnly checkInDate,
        DateOnly checkOutDate,
        int nights,
        int unitQuantity,
        int adults,
        int children,
        int guestCount,
        out decimal unitRateSnapshot,
        out decimal stayCharge,
        out string? error)
    {
        unitRateSnapshot = 0;
        stayCharge = 0;
        error = null;

        if (prices.Count == 0)
        {
            error = "No rate plan price covers the requested stay dates.";
            return false;
        }

        if (pricingBasis == PricingBasis.FlatPerStay)
        {
            var covering = FindPriceForDate(prices, checkInDate);
            if (covering is null)
            {
                error = "No rate plan price covers the check-in date for a flat stay rate.";
                return false;
            }

            if (nights < covering.MinimumStay)
            {
                error = $"Minimum stay for this rate plan is {covering.MinimumStay} night(s).";
                return false;
            }

            unitRateSnapshot = covering.UnitRate;
            stayCharge = covering.UnitRate * unitQuantity;
            return true;
        }

        decimal nightlyTotal = 0;
        var requiredMinimumStay = 1;

        for (var day = checkInDate; day < checkOutDate; day = day.AddDays(1))
        {
            var price = FindPriceForDate(prices, day);
            if (price is null)
            {
                error = $"No rate plan price covers {day:yyyy-MM-dd}.";
                return false;
            }

            requiredMinimumStay = Math.Max(requiredMinimumStay, price.MinimumStay);
            nightlyTotal += NightlyAmount(pricingBasis, price, adults, children, guestCount);
        }

        if (nights < requiredMinimumStay)
        {
            error = $"Minimum stay for this rate plan is {requiredMinimumStay} night(s).";
            return false;
        }

        unitRateSnapshot = Math.Round(nightlyTotal / nights, 2, MidpointRounding.AwayFromZero);
        stayCharge = nightlyTotal * unitQuantity;
        return true;
    }

    private static RatePlanPriceRow? FindPriceForDate(
        IReadOnlyList<RatePlanPriceRow> prices,
        DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        RatePlanPriceRow? fallback = null;

        foreach (var price in prices)
        {
            if (date < price.StartDate || date > price.EndDate)
                continue;

            if (price.DayOfWeek == dayOfWeek)
                return price;

            if (price.DayOfWeek is null)
                fallback ??= price;
        }

        return fallback;
    }

    private static decimal NightlyAmount(
        PricingBasis pricingBasis,
        RatePlanPriceRow price,
        int adults,
        int children,
        int guestCount) =>
        pricingBasis switch
        {
            PricingBasis.PerPersonPerNight when price.AdultRate is not null || price.ChildRate is not null
                => (price.AdultRate ?? 0) * adults + (price.ChildRate ?? 0) * children,
            PricingBasis.PerPersonPerNight => price.UnitRate * Math.Max(guestCount, 1),
            PricingBasis.PerBedPerNight => price.UnitRate * Math.Max(guestCount, 1),
            _ => price.UnitRate
        };
}
