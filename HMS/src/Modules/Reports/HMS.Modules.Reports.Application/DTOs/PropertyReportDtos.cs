namespace HMS.Modules.Reports.Application.DTOs;

public sealed record BookingProfitabilityDto(
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
    decimal OutstandingBalance,
    decimal DirectExpense,
    decimal AllocatedOverhead,
    decimal EstimatedProfit,
    decimal? ProfitPerGuestNight);

public sealed record GuestProfitabilityDto(
    Guid GuestUid,
    string DisplayName,
    int BookingCount,
    int Nights,
    decimal TotalBookingValue,
    decimal DirectExpense,
    decimal AllocatedOverhead,
    decimal EstimatedProfit);

public sealed record OccupancyStayDto(
    Guid GuestUid,
    string DisplayName,
    Guid BookingUid,
    string BookingNumber,
    string Status,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    decimal TotalBookingValue,
    decimal NetPaid,
    decimal OutstandingBalance);

public sealed record PaymentMethodSummaryDto(
    DateOnly ReportMonth,
    string PaymentMethod,
    string Currency,
    int PaymentCount,
    decimal AmountReceived);

public sealed record OutstandingBalanceDto(
    Guid BookingUid,
    string BookingNumber,
    string Status,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string Currency,
    decimal TotalBookingValue,
    decimal NetPaid,
    decimal OutstandingBalance);

public sealed record ExpenseReportDto(
    DateOnly ReportMonth,
    decimal StaffExpenses,
    decimal UtilityExpenses,
    decimal GeneralExpenses,
    decimal TotalExpenses);

public sealed record UtilityReportDto(
    DateOnly ReportMonth,
    string UtilityCode,
    string UtilityName,
    string? UnitOfMeasure,
    decimal TotalUnits,
    decimal TotalCost);
