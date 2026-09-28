using Npgsql;

namespace HMS.Modules.Reports.Infrastructure.Persistence;

public interface IReportsDbConnectionFactory
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
