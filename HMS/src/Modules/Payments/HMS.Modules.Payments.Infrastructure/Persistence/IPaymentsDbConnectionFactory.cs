using Npgsql;

namespace HMS.Modules.Payments.Infrastructure.Persistence;

public interface IPaymentsDbConnectionFactory
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
