namespace HMS.Modules.Identity.Application.Authentication;

/// <summary>
/// Development dummy credentials for the platform super admin.
/// Super admin is not stored in the database and cannot be registered.
/// </summary>
public static class SuperAdminCredentials
{
    public const string Username = "superadmin";
    public const string Password = "SuperAdmin@123";
    public const string Email = "superadmin@hms.local";
    public const string FirstName = "Super";
    public const string LastName = "Admin";

    public static readonly Guid StaffUid =
        Guid.Parse("00000000-0000-0000-0000-000000000001");

    public static bool Matches(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;

        return string.Equals(
                   username.Trim(),
                   Username,
                   StringComparison.OrdinalIgnoreCase)
               && string.Equals(password, Password, StringComparison.Ordinal);
    }
}
