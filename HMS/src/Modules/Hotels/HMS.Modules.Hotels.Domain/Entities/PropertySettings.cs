using HMS.Modules.Hotels.Domain.Common;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class PropertySettings : AuditableEntity
{
    private PropertySettings() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public TimeOnly CheckInTime { get; private set; } = new(14, 0);
    public TimeOnly CheckOutTime { get; private set; } = new(11, 0);
    public string BookingNumberPrefix { get; private set; } = "BKG";
    public string InvoiceNumberPrefix { get; private set; } = "INV";
    public decimal TaxRate { get; private set; }
    public decimal ServiceChargeRate { get; private set; }
    public bool AllowOverbooking { get; private set; }
    public string ExtraSettingsJson { get; private set; } = "{}";

    public static PropertySettings CreateDefault(long organizationId, string actorSubject)
    {
        var settings = new PropertySettings
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId
        };
        settings.MarkCreated(actorSubject);
        return settings;
    }

    public void Update(
        TimeOnly checkInTime,
        TimeOnly checkOutTime,
        string bookingNumberPrefix,
        string invoiceNumberPrefix,
        decimal taxRate,
        decimal serviceChargeRate,
        bool allowOverbooking,
        string extraSettingsJson,
        string actorSubject)
    {
        if (taxRate is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(taxRate));
        if (serviceChargeRate is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(serviceChargeRate));
        ArgumentException.ThrowIfNullOrWhiteSpace(bookingNumberPrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceNumberPrefix);

        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        BookingNumberPrefix = bookingNumberPrefix.Trim().ToUpperInvariant();
        InvoiceNumberPrefix = invoiceNumberPrefix.Trim().ToUpperInvariant();
        TaxRate = taxRate;
        ServiceChargeRate = serviceChargeRate;
        AllowOverbooking = allowOverbooking;
        ExtraSettingsJson = string.IsNullOrWhiteSpace(extraSettingsJson) ? "{}" : extraSettingsJson;
        MarkModified(actorSubject);
    }

    public static PropertySettings Rehydrate(
        long id, Guid uid, long organizationId, long propertyId,
        TimeOnly checkInTime, TimeOnly checkOutTime,
        string bookingNumberPrefix, string invoiceNumberPrefix,
        decimal taxRate, decimal serviceChargeRate, bool allowOverbooking,
        string extraSettingsJson, bool isActive, bool isArchived,
        DateTimeOffset creationDate, string? createdBy,
        DateTimeOffset? modifiedDate, string? modifiedBy) =>
        new()
        {
            Id = id,
            Uid = uid,
            OrganizationId = organizationId,
            PropertyId = propertyId,
            CheckInTime = checkInTime,
            CheckOutTime = checkOutTime,
            BookingNumberPrefix = bookingNumberPrefix,
            InvoiceNumberPrefix = invoiceNumberPrefix,
            TaxRate = taxRate,
            ServiceChargeRate = serviceChargeRate,
            AllowOverbooking = allowOverbooking,
            ExtraSettingsJson = extraSettingsJson,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };
}

