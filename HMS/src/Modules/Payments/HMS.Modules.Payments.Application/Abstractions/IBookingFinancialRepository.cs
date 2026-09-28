using HMS.Modules.Payments.Application.DTOs;

namespace HMS.Modules.Payments.Application.Abstractions;

public interface IBookingFinancialRepository
{
    Task<BookingFinancialSummaryDto?> GetSummaryAsync(long bookingId, CancellationToken cancellationToken);
    Task<BookingInvoiceDto?> GetInvoiceAsync(long bookingId, CancellationToken cancellationToken);
}
