using Npgsql;

namespace HMS.Modules.Staff.Infrastructure.Persistence;

public interface IStaffDbConnectionFactory
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken);
}
