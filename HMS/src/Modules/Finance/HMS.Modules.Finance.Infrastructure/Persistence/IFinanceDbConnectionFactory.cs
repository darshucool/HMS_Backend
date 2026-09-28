using Npgsql;

namespace HMS.Modules.Finance.Infrastructure.Persistence;

public interface IFinanceDbConnectionFactory
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
