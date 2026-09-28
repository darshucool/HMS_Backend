using Microsoft.Extensions.Configuration;
using Npgsql;

namespace HMS.Modules.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbConnectionFactory : IPaymentsDbConnectionFactory, IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public PaymentsDbConnectionFactory(IConfiguration configuration)
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
