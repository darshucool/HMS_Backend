namespace HMS.Modules.Identity.Domain.Entities;

public sealed class StaffUser
{
    private StaffUser()
    {
    }

    public Guid Uid { get; private set; }

    /// <summary>
    /// Stored for schema compatibility. Always equals the login email.
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    public string NormalizedUsername { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string? LastName { get; private set; }

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public bool IsLocked { get; private set; }

    public int FailedLoginCount { get; private set; }

    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public DateTimeOffset? LastLoginUtc { get; private set; }

    public string FullName =>
        string.Join(
            " ",
            new[] { FirstName, LastName }
                .Where(value => !string.IsNullOrWhiteSpace(value)));

    public bool IsCurrentlyLocked(DateTimeOffset currentTime)
    {
        return IsLocked ||
               (LockedUntilUtc.HasValue &&
                LockedUntilUtc.Value > currentTime);
    }

    public static StaffUser Create(
        string firstName,
        string? lastName,
        string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var cleanedEmail = email.Trim();
        if (!cleanedEmail.Contains('@'))
            throw new ArgumentException("Email is invalid.", nameof(email));

        var normalizedEmail = cleanedEmail.ToUpperInvariant();

        return new StaffUser
        {
            Uid = Guid.NewGuid(),
            Username = cleanedEmail,
            NormalizedUsername = normalizedEmail,
            FirstName = firstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
            Email = cleanedEmail,
            NormalizedEmail = normalizedEmail,
            IsActive = true
        };
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
