using HMS.Modules.Booking.Domain.Common;
using HMS.Modules.Booking.Domain.Enums;

namespace HMS.Modules.Booking.Domain.Entities;

public sealed class Booking : AuditableEntity
{
    private Booking() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public Guid PropertyUid { get; private set; }
    public string BookingNumber { get; private set; } = string.Empty;
    public long? LeadGuestId { get; private set; }
    public Guid? LeadGuestUid { get; private set; }
    public BookingSource BookingSource { get; private set; } = BookingSource.Direct;
    public string? ExternalReference { get; private set; }
    public DateOnly CheckInDate { get; private set; }
    public DateOnly CheckOutDate { get; private set; }
    public int Adults { get; private set; } = 1;
    public int Children { get; private set; }
    public int Infants { get; private set; }
    public string Currency { get; private set; } = "LKR";
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ServiceCharge { get; private set; }
    public decimal? QuotedTotal { get; private set; }
    public TimeOnly? ArrivalTime { get; private set; }
    public TimeOnly? DepartureTime { get; private set; }
    public string? SpecialRequests { get; private set; }
    public string? InternalNotes { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CheckedInAt { get; private set; }
    public DateTimeOffset? CheckedOutAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? PreviousStatus { get; private set; }

    public static Booking Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        string bookingNumber,
        long? leadGuestId,
        Guid? leadGuestUid,
        BookingSource bookingSource,
        string? externalReference,
        DateOnly checkInDate,
        DateOnly checkOutDate,
        int adults,
        int children,
        int infants,
        string currency,
        decimal discountAmount,
        decimal taxAmount,
        decimal serviceCharge,
        decimal? quotedTotal,
        TimeOnly? arrivalTime,
        TimeOnly? departureTime,
        string? specialRequests,
        string? internalNotes,
        string actorSubject)
    {
        if (checkOutDate <= checkInDate)
            throw new ArgumentException("Check-out must be after check-in.", nameof(checkOutDate));

        if (adults <= 0 || children < 0 || infants < 0)
            throw new ArgumentException("Adults must be greater than zero and children/infants cannot be negative.");

        if (discountAmount < 0 || taxAmount < 0 || serviceCharge < 0 || (quotedTotal ?? 0) < 0)
            throw new ArgumentException("Booking amounts cannot be negative.");

        var cleanedCurrency = string.IsNullOrWhiteSpace(currency) ? "LKR" : currency.Trim().ToUpperInvariant();
        if (cleanedCurrency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

        var booking = new Booking
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            BookingNumber = bookingNumber,
            LeadGuestId = leadGuestId,
            LeadGuestUid = leadGuestUid,
            BookingSource = bookingSource,
            ExternalReference = Optional(externalReference, 100),
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            Adults = adults,
            Children = children,
            Infants = infants,
            Currency = cleanedCurrency,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            ServiceCharge = serviceCharge,
            QuotedTotal = quotedTotal,
            ArrivalTime = arrivalTime,
            DepartureTime = departureTime,
            SpecialRequests = Optional(specialRequests, 4000),
            InternalNotes = Optional(internalNotes, 4000)
        };

        booking.MarkCreated(actorSubject);
        return booking;
    }

    public void Update(
        long? leadGuestId,
        Guid? leadGuestUid,
        BookingSource bookingSource,
        BookingStatus status,
        string? externalReference,
        DateOnly checkInDate,
        DateOnly checkOutDate,
        int adults,
        int children,
        int infants,
        string currency,
        decimal discountAmount,
        decimal taxAmount,
        decimal serviceCharge,
        decimal? quotedTotal,
        TimeOnly? arrivalTime,
        TimeOnly? departureTime,
        string? specialRequests,
        string? internalNotes,
        string? cancellationReason,
        string actorSubject)
    {
        if (checkOutDate <= checkInDate)
            throw new ArgumentException("Check-out must be after check-in.", nameof(checkOutDate));

        if (adults <= 0 || children < 0 || infants < 0)
            throw new ArgumentException("Adults must be greater than zero and children/infants cannot be negative.");

        if (discountAmount < 0 || taxAmount < 0 || serviceCharge < 0 || (quotedTotal ?? 0) < 0)
            throw new ArgumentException("Booking amounts cannot be negative.");

        if (status == BookingStatus.Cancelled && string.IsNullOrWhiteSpace(cancellationReason))
            throw new ArgumentException("A cancellation reason is required.", nameof(cancellationReason));

        var cleanedCurrency = string.IsNullOrWhiteSpace(currency) ? "LKR" : currency.Trim().ToUpperInvariant();
        if (cleanedCurrency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

        PreviousStatus = null;
        if (status != Status)
        {
            PreviousStatus = Status.ToDatabaseValue();
            var now = DateTimeOffset.UtcNow;
            if (status == BookingStatus.Confirmed)
                ConfirmedAt = now;
            if (status == BookingStatus.CheckedIn)
                CheckedInAt = now;
            if (status == BookingStatus.CheckedOut)
                CheckedOutAt = now;
            if (status == BookingStatus.Cancelled)
                CancelledAt = now;
        }

        LeadGuestId = leadGuestId;
        LeadGuestUid = leadGuestUid;
        BookingSource = bookingSource;
        Status = status;
        ExternalReference = Optional(externalReference, 100);
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Adults = adults;
        Children = children;
        Infants = infants;
        Currency = cleanedCurrency;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        ServiceCharge = serviceCharge;
        QuotedTotal = quotedTotal;
        ArrivalTime = arrivalTime;
        DepartureTime = departureTime;
        SpecialRequests = Optional(specialRequests, 4000);
        InternalNotes = Optional(internalNotes, 4000);
        CancellationReason = Optional(cancellationReason, 2000);
        MarkModified(actorSubject);
    }

    public void Confirm(string actorSubject)
    {
        if (Status == BookingStatus.Confirmed)
            throw new ArgumentException("This booking is already confirmed.");

        if (Status is BookingStatus.Cancelled or BookingStatus.CheckedIn
            or BookingStatus.CheckedOut or BookingStatus.Completed or BookingStatus.NoShow)
        {
            throw new ArgumentException("This booking cannot be confirmed.");
        }

        PreviousStatus = Status.ToDatabaseValue();
        Status = BookingStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        MarkModified(actorSubject);
    }

    public void CheckIn(string actorSubject)
    {
        if (Status == BookingStatus.CheckedIn)
            throw new ArgumentException("This booking is already checked in.");

        if (Status != BookingStatus.Confirmed)
            throw new ArgumentException("Only confirmed bookings can be checked in.");

        PreviousStatus = Status.ToDatabaseValue();
        Status = BookingStatus.CheckedIn;
        CheckedInAt = DateTimeOffset.UtcNow;
        MarkModified(actorSubject);
    }

    public void CheckOut(string actorSubject)
    {
        if (Status == BookingStatus.CheckedOut)
            throw new ArgumentException("This booking is already checked out.");

        if (Status != BookingStatus.CheckedIn)
            throw new ArgumentException("Only a checked-in booking can be checked out.");

        PreviousStatus = Status.ToDatabaseValue();
        Status = BookingStatus.CheckedOut;
        CheckedOutAt = DateTimeOffset.UtcNow;
        MarkModified(actorSubject);
    }

    public void Cancel(string reason, string actorSubject)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A cancellation reason is required.", nameof(reason));

        if (Status == BookingStatus.Cancelled)
            throw new ArgumentException("This booking is already cancelled.");

        if (Status is BookingStatus.CheckedOut or BookingStatus.Completed)
            throw new ArgumentException("This booking cannot be cancelled.");

        PreviousStatus = Status.ToDatabaseValue();
        Status = BookingStatus.Cancelled;
        CancellationReason = Optional(reason, 2000);
        CancelledAt = DateTimeOffset.UtcNow;
        MarkModified(actorSubject);
    }

    public static Booking Rehydrate(
        long id,
        Guid uid,
        long organizationId,
        long propertyId,
        Guid propertyUid,
        string bookingNumber,
        long? leadGuestId,
        Guid? leadGuestUid,
        BookingSource bookingSource,
        BookingStatus status,
        string? externalReference,
        DateOnly checkInDate,
        DateOnly checkOutDate,
        int adults,
        int children,
        int infants,
        string currency,
        decimal discountAmount,
        decimal taxAmount,
        decimal serviceCharge,
        decimal? quotedTotal,
        TimeOnly? arrivalTime,
        TimeOnly? departureTime,
        string? specialRequests,
        string? internalNotes,
        string? cancellationReason,
        DateTimeOffset? confirmedAt,
        DateTimeOffset? checkedInAt,
        DateTimeOffset? checkedOutAt,
        DateTimeOffset? cancelledAt,
        bool isActive,
        bool isArchived,
        DateTimeOffset creationDate,
        string? createdBy,
        DateTimeOffset? modifiedDate,
        string? modifiedBy) =>
        new()
        {
            Id = id,
            Uid = uid,
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            BookingNumber = bookingNumber,
            LeadGuestId = leadGuestId,
            LeadGuestUid = leadGuestUid,
            BookingSource = bookingSource,
            Status = status,
            ExternalReference = externalReference,
            CheckInDate = checkInDate,
            CheckOutDate = checkOutDate,
            Adults = adults,
            Children = children,
            Infants = infants,
            Currency = currency,
            DiscountAmount = discountAmount,
            TaxAmount = taxAmount,
            ServiceCharge = serviceCharge,
            QuotedTotal = quotedTotal,
            ArrivalTime = arrivalTime,
            DepartureTime = departureTime,
            SpecialRequests = specialRequests,
            InternalNotes = internalNotes,
            CancellationReason = cancellationReason,
            ConfirmedAt = confirmedAt,
            CheckedInAt = checkedInAt,
            CheckedOutAt = checkedOutAt,
            CancelledAt = cancelledAt,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };

    private static string? Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.");

        return trimmed;
    }
}
