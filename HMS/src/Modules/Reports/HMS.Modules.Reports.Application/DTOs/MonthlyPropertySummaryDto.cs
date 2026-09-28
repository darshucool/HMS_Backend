namespace HMS.Modules.Reports.Application.DTOs;

public sealed record MonthlyPropertySummaryDto(
    Guid PropertyUid,
    string PropertyName,
    DateOnly ReportMonth,
    decimal BookingRevenue,
    decimal BookingExtraIncome,
    decimal OtherIncome,
    decimal TotalIncome,
    decimal StaffExpenses,
    decimal UtilityExpenses,
    decimal GeneralExpenses,
    decimal TotalExpenses,
    decimal NetProfit);
