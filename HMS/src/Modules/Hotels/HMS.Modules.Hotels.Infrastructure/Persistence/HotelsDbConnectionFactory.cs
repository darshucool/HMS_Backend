using Microsoft.Extensions.Configuration;
using Npgsql;

namespace HMS.Modules.Hotels.Infrastructure.Persistence;

public sealed class HotelsDbConnectionFactory : IHotelsDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public HotelsDbConnectionFactory(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HotelDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'HotelDatabase' is not configured.");

        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async ValueTask<NpgsqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}

