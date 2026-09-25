namespace HMS.Modules.Finance.Application.DTOs;

public sealed record ExpenseDto(
    Guid Uid,
    Guid PropertyUid,
    Guid ExpenseCategoryUid,
    string ExpenseCategoryName,
    Guid? SupplierUid,
    string? SupplierName,
    Guid? BookingUid,
    DateOnly ExpenseDate,
    string Description,
    decimal Amount,
    string Currency,
    string? PaymentMethod,
    string? ReferenceNumber,
    string? ReceiptUrl,
    string? Notes,
    DateTimeOffset CreationDate);
