using HMS.Modules.Finance.Domain.Common;

namespace HMS.Modules.Finance.Domain.Entities;

public sealed class UtilityBill : AuditableEntity
{
    private UtilityBill() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public long UtilityTypeId { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public decimal? PreviousReading { get; private set; }
    public decimal? CurrentReading { get; private set; }
    public decimal? UnitsUsed { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "LKR";
    public DateOnly? DueDate { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid PropertyUid { get; private set; }
    public Guid UtilityTypeUid { get; private set; }

    public static UtilityBill Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long utilityTypeId,
        Guid utilityTypeUid,
        DateOnly periodStart,
        DateOnly periodEnd,
        decimal? previousReading,
        decimal? currentReading,
        decimal? unitsUsed,
        decimal amount,
        string currency,
        DateOnly? dueDate,
        DateTimeOffset? paidAt,
        string? referenceNumber,
        string? notes,
        string actorSubject)
    {
        if (periodEnd < periodStart)
            throw new ArgumentException("Period end cannot be before period start.", nameof(periodEnd));

        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (previousReading is not null && currentReading is not null && currentReading < previousReading)
            throw new ArgumentException("Current reading cannot be less than the previous reading.");

        var calculatedUnits = previousReading is not null && currentReading is not null
            ? currentReading - previousReading
            : unitsUsed;

        if (calculatedUnits is < 0)
            throw new ArgumentException("Units used cannot be negative.");

        var cleanedCurrency = string.IsNullOrWhiteSpace(currency) ? "LKR" : currency.Trim().ToUpperInvariant();
        if (cleanedCurrency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

        var bill = new UtilityBill
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            UtilityTypeId = utilityTypeId,
            UtilityTypeUid = utilityTypeUid,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            PreviousReading = previousReading,
            CurrentReading = currentReading,
            UnitsUsed = calculatedUnits,
            Amount = amount,
            Currency = cleanedCurrency,
            DueDate = dueDate,
            PaidAt = paidAt,
            ReferenceNumber = Optional(referenceNumber, 150),
            Notes = Optional(notes, 4000)
        };

        bill.MarkCreated(actorSubject);
        return bill;
    }

    public void Update(
    long utilityTypeId,
    Guid utilityTypeUid,
    DateOnly periodStart,
    DateOnly periodEnd,
    decimal? previousReading,
    decimal? currentReading,
    decimal? unitsUsed,
    decimal amount,
    string currency,
    DateOnly? dueDate,
    DateTimeOffset? paidAt,
    string? referenceNumber,
    string? notes,
    string actorSubject)
{
    if (periodEnd < periodStart)
        throw new ArgumentException("Period end cannot be before period start.", nameof(periodEnd));

    if (amount < 0)
        throw new ArgumentException("Amount cannot be negative.", nameof(amount));

    if (previousReading is not null && currentReading is not null && currentReading < previousReading)
        throw new ArgumentException("Current reading cannot be less than the previous reading.");

    var calculatedUnits = previousReading is not null && currentReading is not null
        ? currentReading - previousReading
        : unitsUsed;

    if (calculatedUnits is < 0)
        throw new ArgumentException("Units used cannot be negative.");

    var cleanedCurrency = string.IsNullOrWhiteSpace(currency) ? "LKR" : currency.Trim().ToUpperInvariant();
    if (cleanedCurrency.Length != 3)
        throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

    UtilityTypeId = utilityTypeId;
    UtilityTypeUid = utilityTypeUid;
    PeriodStart = periodStart;
    PeriodEnd = periodEnd;
    PreviousReading = previousReading;
    CurrentReading = currentReading;
    UnitsUsed = calculatedUnits;
    Amount = amount;
    Currency = cleanedCurrency;
    DueDate = dueDate;
    PaidAt = paidAt;
    ReferenceNumber = Optional(referenceNumber, 150);
    Notes = Optional(notes, 4000);
    MarkModified(actorSubject);
}

public static UtilityBill Rehydrate(
    long id,
    Guid uid,
    long organizationId,
    long propertyId,
    Guid propertyUid,
    long utilityTypeId,
    Guid utilityTypeUid,
    DateOnly periodStart,
    DateOnly periodEnd,
    decimal? previousReading,
    decimal? currentReading,
    decimal? unitsUsed,
    decimal amount,
    string currency,
    DateOnly? dueDate,
    DateTimeOffset? paidAt,
    string? referenceNumber,
    string? notes,
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
        UtilityTypeId = utilityTypeId,
        UtilityTypeUid = utilityTypeUid,
        PeriodStart = periodStart,
        PeriodEnd = periodEnd,
        PreviousReading = previousReading,
        CurrentReading = currentReading,
        UnitsUsed = unitsUsed,
        Amount = amount,
        Currency = currency,
        DueDate = dueDate,
        PaidAt = paidAt,
        ReferenceNumber = referenceNumber,
        Notes = notes,
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
