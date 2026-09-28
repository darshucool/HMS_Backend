using HMS.Modules.Finance.Domain.Enums;

namespace HMS.Modules.Finance.Api.Contracts;

public sealed record CreateExpenseRequest(
    Guid ExpenseCategoryUid,
    DateOnly ExpenseDate,
    string Description,
    decimal Amount,
    Guid? SupplierUid = null,
    Guid? BookingUid = null,
    string Currency = "LKR",
    FinancePaymentMethod? PaymentMethod = null,
    string? ReferenceNumber = null,
    string? ReceiptUrl = null,
    string? Notes = null);

public sealed record UpdateExpenseRequest(
    Guid ExpenseCategoryUid,
    DateOnly ExpenseDate,
    string Description,
    decimal Amount,
    Guid? SupplierUid = null,
    Guid? BookingUid = null,
    string Currency = "LKR",
    FinancePaymentMethod? PaymentMethod = null,
    string? ReferenceNumber = null,
    string? ReceiptUrl = null,
    string? Notes = null);