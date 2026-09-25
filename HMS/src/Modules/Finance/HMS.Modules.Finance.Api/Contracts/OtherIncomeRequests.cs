using HMS.Modules.Finance.Domain.Enums;

namespace HMS.Modules.Finance.Api.Contracts;

public sealed record CreateOtherIncomeRequest(
    Guid IncomeCategoryUid,
    DateOnly IncomeDate,
    string Description,
    decimal Amount,
    Guid? BookingUid = null,
    string Currency = "LKR",
    FinancePaymentMethod? PaymentMethod = null,
    string? ReferenceNumber = null,
    string? Notes = null);
