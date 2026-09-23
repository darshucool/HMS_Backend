using Npgsql;

namespace HMS.Modules.Guests.Infrastructure.Persistence;

public interface IGuestsDbConnectionFactory
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
