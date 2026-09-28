using HMS.Modules.Payments.Domain.Enums;

namespace HMS.Modules.Payments.Api.Contracts;

public sealed record UpsertBookingChargeRequest(
    Guid ChargeTypeUid,
    string Description,
    decimal UnitPrice,
    decimal Quantity = 1,
    decimal DiscountAmount = 0,
    decimal TaxAmount = 0,
    Guid? BookingUnitUid = null,
    DateOnly? ServiceDate = null,
    string? Notes = null);

public sealed record CreateBookingPaymentRequest(
    decimal Amount,
    BookingPaymentMethod PaymentMethod = BookingPaymentMethod.Cash,
    BookingPaymentType PaymentType = BookingPaymentType.Payment,
    BookingPaymentStatus Status = BookingPaymentStatus.Completed,
    string Currency = "LKR",
    string? ReferenceNumber = null,
    DateTimeOffset? PaidAt = null,
    string? Notes = null);

public sealed record RefundBookingPaymentRequest(
    decimal Amount,
    string Reason);
