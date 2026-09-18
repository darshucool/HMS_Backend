using Npgsql;

namespace HMS.Modules.Hotels.Infrastructure.Persistence;

public interface IHotelsDbConnectionFactory
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}

