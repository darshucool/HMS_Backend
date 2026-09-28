using HMS.Modules.Reports.Application.DTOs;

namespace HMS.Modules.Reports.Application.Abstractions;

public interface IPropertyReportRepository
{
    Task<IReadOnlyList<BookingProfitabilityDto>> GetBookingProfitabilityAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<GuestProfitabilityDto>> GetGuestProfitabilityAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OccupancyStayDto>> GetOccupancyAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PaymentMethodSummaryDto>> GetPaymentSummaryAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OutstandingBalanceDto>> GetOutstandingBalancesAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);

    Task<ExpenseReportDto?> GetExpensesAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UtilityReportDto>> GetUtilitiesAsync(
        Guid propertyUid,
        DateOnly reportMonth,
        CancellationToken cancellationToken);
}
