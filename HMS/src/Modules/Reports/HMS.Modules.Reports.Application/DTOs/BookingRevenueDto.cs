namespace HMS.Modules.Reports.Application.DTOs;

public sealed record BookingRevenueDto(
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
