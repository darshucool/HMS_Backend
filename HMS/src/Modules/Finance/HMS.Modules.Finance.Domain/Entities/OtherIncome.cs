using HMS.Modules.Finance.Domain.Common;
using HMS.Modules.Finance.Domain.Enums;

namespace HMS.Modules.Finance.Domain.Entities;

public sealed class OtherIncome : AuditableEntity
{
    private OtherIncome() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public long IncomeCategoryId { get; private set; }
    public long? BookingId { get; private set; }
    public DateOnly IncomeDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "LKR";
    public FinancePaymentMethod? PaymentMethod { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid PropertyUid { get; private set; }
    public Guid IncomeCategoryUid { get; private set; }
    public Guid? BookingUid { get; private set; }

    public static OtherIncome Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long incomeCategoryId,
        Guid incomeCategoryUid,
        long? bookingId,
        Guid? bookingUid,
        DateOnly incomeDate,
        string description,
        decimal amount,
        string currency,
        FinancePaymentMethod? paymentMethod,
        string? referenceNumber,
        string? notes,
        string actorSubject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (description.Trim().Length > 300)
            throw new ArgumentException("Description cannot exceed 300 characters.", nameof(description));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        var cleanedCurrency = string.IsNullOrWhiteSpace(currency) ? "LKR" : currency.Trim().ToUpperInvariant();
        if (cleanedCurrency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

        var income = new OtherIncome
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            IncomeCategoryId = incomeCategoryId,
            IncomeCategoryUid = incomeCategoryUid,
            BookingId = bookingId,
            BookingUid = bookingUid,
            IncomeDate = incomeDate,
            Description = description.Trim(),
            Amount = amount,
            Currency = cleanedCurrency,
            PaymentMethod = paymentMethod,
            ReferenceNumber = Optional(referenceNumber, 150),
            Notes = Optional(notes, 4000)
        };

        income.MarkCreated(actorSubject);
        return income;
    }

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
