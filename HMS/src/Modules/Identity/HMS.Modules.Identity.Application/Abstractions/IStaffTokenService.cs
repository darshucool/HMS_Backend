namespace HMS.Modules.Identity.Application.Abstractions;

public interface IStaffTokenService
{
    StaffTokenResult Generate(
        StaffLoginRecord staff,
        DateTimeOffset currentTime);
}

public sealed record StaffTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);

public interface IRefreshTokenStore
{
    Task<IssuedRefreshToken> IssueAsync(
        Guid staffUid,
        Guid? propertyUid,
        DateTimeOffset currentTime,
        CancellationToken cancellationToken);

    Task<StoredRefreshToken?> TakeValidAsync(
        string refreshToken,
        DateTimeOffset currentTime,
        CancellationToken cancellationToken);
}

public sealed record IssuedRefreshToken(
    string RefreshToken,
    DateTimeOffset ExpiresAtUtc);

public sealed record StoredRefreshToken(
    Guid StaffUid,
    Guid? PropertyUid);
