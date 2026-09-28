using Npgsql;

namespace HMS.Modules.Booking.Infrastructure.Persistence;

public interface IBookingDbConnectionFactory
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
