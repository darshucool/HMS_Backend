using HMS.Modules.Finance.Domain.Common;
using HMS.Modules.Finance.Domain.Enums;

namespace HMS.Modules.Finance.Domain.Entities;

public sealed class Expense : AuditableEntity
{
    private Expense() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public long ExpenseCategoryId { get; private set; }
    public long? SupplierId { get; private set; }
    public long? BookingId { get; private set; }
    public DateOnly ExpenseDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "LKR";
    public FinancePaymentMethod? PaymentMethod { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? ReceiptUrl { get; private set; }
    public string? Notes { get; private set; }
    public Guid PropertyUid { get; private set; }
    public Guid ExpenseCategoryUid { get; private set; }
    public Guid? SupplierUid { get; private set; }
    public Guid? BookingUid { get; private set; }

    public static Expense Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long expenseCategoryId,
        Guid expenseCategoryUid,
        long? supplierId,
        Guid? supplierUid,
        long? bookingId,
        Guid? bookingUid,
        DateOnly expenseDate,
        string description,
        decimal amount,
        string currency,
        FinancePaymentMethod? paymentMethod,
        string? referenceNumber,
        string? receiptUrl,
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

        var expense = new Expense
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            ExpenseCategoryId = expenseCategoryId,
            ExpenseCategoryUid = expenseCategoryUid,
            SupplierId = supplierId,
            SupplierUid = supplierUid,
            BookingId = bookingId,
            BookingUid = bookingUid,
            ExpenseDate = expenseDate,
            Description = description.Trim(),
            Amount = amount,
            Currency = cleanedCurrency,
            PaymentMethod = paymentMethod,
            ReferenceNumber = Optional(referenceNumber, 150),
            ReceiptUrl = Optional(receiptUrl, 2000),
            Notes = Optional(notes, 4000)
        };

        expense.MarkCreated(actorSubject);
        return expense;
    }

    public void Update(
    long expenseCategoryId,
    Guid expenseCategoryUid,
    long? supplierId,
    Guid? supplierUid,
    long? bookingId,
    Guid? bookingUid,
    DateOnly expenseDate,
    string description,
    decimal amount,
    string currency,
    FinancePaymentMethod? paymentMethod,
    string? referenceNumber,
    string? receiptUrl,
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

    ExpenseCategoryId = expenseCategoryId;
    ExpenseCategoryUid = expenseCategoryUid;
    SupplierId = supplierId;
    SupplierUid = supplierUid;
    BookingId = bookingId;
    BookingUid = bookingUid;
    ExpenseDate = expenseDate;
    Description = description.Trim();
    Amount = amount;
    Currency = cleanedCurrency;
    PaymentMethod = paymentMethod;
    ReferenceNumber = Optional(referenceNumber, 150);
    ReceiptUrl = Optional(receiptUrl, 2000);
    Notes = Optional(notes, 4000);
    MarkModified(actorSubject);
}

public static Expense Rehydrate(
    long id,
    Guid uid,
    long organizationId,
    long propertyId,
    Guid propertyUid,
    long expenseCategoryId,
    Guid expenseCategoryUid,
    long? supplierId,
    Guid? supplierUid,
    long? bookingId,
    Guid? bookingUid,
    DateOnly expenseDate,
    string description,
    decimal amount,
    string currency,
    FinancePaymentMethod? paymentMethod,
    string? referenceNumber,
    string? receiptUrl,
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
        ExpenseCategoryId = expenseCategoryId,
        ExpenseCategoryUid = expenseCategoryUid,
        SupplierId = supplierId,
        SupplierUid = supplierUid,
        BookingId = bookingId,
        BookingUid = bookingUid,
        ExpenseDate = expenseDate,
        Description = description,
        Amount = amount,
        Currency = currency,
        PaymentMethod = paymentMethod,
        ReferenceNumber = referenceNumber,
        ReceiptUrl = receiptUrl,
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
