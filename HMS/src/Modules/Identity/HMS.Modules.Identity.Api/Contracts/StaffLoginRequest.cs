namespace HMS.Modules.Identity.Api.Contracts;

public sealed record SuperAdminLoginRequest(
    string Email,
    string Password);

public sealed record StaffLoginRequest(
    string Email,
    string Password,
    Guid? PropertyUid = null);

public sealed record InvitePropertyAdminRequest(
    Guid PropertyUid,
    string Email,
    string FirstName,
    string? LastName = null);

public sealed record ResetAdminCredentialsRequest(
    string Password);

public sealed record RegisterAdminRequest(
    Guid PropertyUid,
    string Email,
    string Password,
    string FirstName,
    string? LastName = null);

public sealed record RegisterStaffRequest(
    Guid PropertyUid,
    string Email,
    string Password,
    string FirstName,
    string? LastName = null,
    string Role = "FRONT_DESK");
