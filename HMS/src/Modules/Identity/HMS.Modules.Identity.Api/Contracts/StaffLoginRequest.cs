namespace HMS.Modules.Identity.Api.Contracts;

public sealed record SuperAdminLoginRequest(
    string Username,
    string Password);

public sealed record StaffLoginRequest(
    Guid PropertyUid,
    string Username,
    string Password);

public sealed record RegisterAdminRequest(
    Guid PropertyUid,
    string Username,
    string Password,
    string FirstName,
    string? LastName = null,
    string? Email = null);

public sealed record RegisterStaffRequest(
    Guid PropertyUid,
    string Username,
    string Password,
    string FirstName,
    string? LastName = null,
    string? Email = null,
    string Role = "FRONT_DESK");
