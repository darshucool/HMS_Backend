namespace HMS.Modules.Payments.Application.DTOs;

public sealed record BookingFinancialSummaryDto(
    Guid BookingUid,
    string BookingNumber,
    string Status,
    string BookingSource,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    int Adults,
    int Children,
    int Infants,
    string Currency,
    decimal RoomRevenue,
    decimal ExtraIncome,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal TotalBookingValue,
    decimal PaymentsReceived,
    decimal RefundsPaid,
    decimal NetPaid,
    decimal OutstandingBalance);

public sealed record BookingInvoicePartyDto(
    string Name,
    string? Phone,
    string? Email,
    string? Address);

public sealed record BookingInvoiceLineDto(
    string LineType,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount);

public sealed record BookingInvoicePaymentDto(
    Guid Uid,
    DateTimeOffset PaidAt,
    string PaymentMethod,
    string PaymentType,
    decimal Amount,
    string Status,
    string? ReferenceNumber);

public sealed record BookingInvoiceRefundDto(
    Guid Uid,
    Guid PaymentUid,
    DateTimeOffset RefundedAt,
    decimal Amount,
    string Status,
    string Reason);

public sealed record BookingInvoiceDto(
    string InvoiceNumber,
    DateOnly InvoiceDate,
    Guid BookingUid,
    string BookingNumber,
    string Status,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    int Adults,
    int Children,
    int Infants,
    string Currency,
    BookingInvoicePartyDto Property,
    BookingInvoicePartyDto? BillTo,
    IReadOnlyList<BookingInvoiceLineDto> Lines,
    decimal RoomRevenue,
    decimal ExtraIncome,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal ServiceCharge,
    decimal TotalBookingValue,
    IReadOnlyList<BookingInvoicePaymentDto> Payments,
    IReadOnlyList<BookingInvoiceRefundDto> Refunds,
    decimal PaymentsReceived,
    decimal RefundsPaid,
    decimal NetPaid,
    decimal OutstandingBalance);
