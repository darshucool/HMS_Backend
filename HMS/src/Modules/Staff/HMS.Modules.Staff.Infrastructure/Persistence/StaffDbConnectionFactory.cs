using Microsoft.Extensions.Configuration;
using Npgsql;

namespace HMS.Modules.Staff.Infrastructure.Persistence;

public sealed class StaffDbConnectionFactory : IStaffDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public StaffDbConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HotelDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'HotelDatabase' is not configured.");

        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken) =>
        await _dataSource.OpenConnectionAsync(cancellationToken);

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}
