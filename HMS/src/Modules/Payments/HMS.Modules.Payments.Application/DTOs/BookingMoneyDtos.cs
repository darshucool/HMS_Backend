namespace HMS.Modules.Payments.Application.DTOs;

public sealed record BookingChargeDto(
    Guid Uid,
    Guid BookingUid,
    Guid? BookingUnitUid,
    Guid ChargeTypeUid,
    string ChargeTypeName,
    DateOnly ServiceDate,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string? Notes,
    DateTimeOffset CreationDate);

public sealed record BookingPaymentDto(
    Guid Uid,
    Guid BookingUid,
    string PaymentMethod,
    string PaymentType,
    decimal Amount,
    string Currency,
    string Status,
    string? ReferenceNumber,
    DateTimeOffset PaidAt,
    string? Notes,
    DateTimeOffset CreationDate);

public sealed record BookingRefundDto(
    Guid Uid,
    Guid PaymentUid,
    Guid BookingUid,
    decimal Amount,
    string Status,
    string Reason,
    DateTimeOffset RefundedAt);
