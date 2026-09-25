using System.Security.Cryptography;
using System.Text;
using Dapper;
using HMS.Modules.Identity.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace HMS.Modules.Identity.Infrastructure.Authentication;

internal sealed class RefreshTokenStore(
    NpgsqlDataSource dataSource,
    IConfiguration configuration) : IRefreshTokenStore
{
    public async Task<IssuedRefreshToken> IssueAsync(
        Guid staffUid,
        Guid? propertyUid,
        DateTimeOffset currentTime,
        CancellationToken cancellationToken)
    {
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var expiresAt = currentTime.AddDays(RefreshTokenDays());

        const string sql = """
            INSERT INTO identity.staff_refresh_token
                (staff_uid, property_uid, token_hash, expires_at_utc, created_at_utc)
            VALUES
                (@StaffUid, @PropertyUid, @TokenHash, @ExpiresAtUtc, NOW());
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                StaffUid = staffUid,
                PropertyUid = propertyUid is null || propertyUid == Guid.Empty ? null : propertyUid,
                TokenHash = Hash(refreshToken),
                ExpiresAtUtc = expiresAt
            },
            cancellationToken: cancellationToken));

        return new IssuedRefreshToken(refreshToken, expiresAt);
    }

    public async Task<StoredRefreshToken?> TakeValidAsync(
        string refreshToken,
        DateTimeOffset currentTime,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE identity.staff_refresh_token
            SET revoked_at_utc = NOW()
            WHERE token_hash = @TokenHash
              AND revoked_at_utc IS NULL
              AND expires_at_utc > @CurrentTime
            RETURNING staff_uid AS StaffUid, property_uid AS PropertyUid;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<StoredRefreshToken>(
            new CommandDefinition(
                sql,
                new
                {
                    TokenHash = Hash(refreshToken),
                    CurrentTime = currentTime
                },
                cancellationToken: cancellationToken));
    }

    private int RefreshTokenDays() =>
        int.TryParse(configuration["Jwt:RefreshTokenDays"], out var days) && days > 0
            ? days
            : 14;

    private static string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
