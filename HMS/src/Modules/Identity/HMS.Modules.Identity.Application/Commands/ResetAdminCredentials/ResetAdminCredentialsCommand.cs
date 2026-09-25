using HMS.Modules.Identity.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HMS.Modules.Identity.Application.Commands.ResetAdminCredentials;

public sealed record ResetAdminCredentialsCommand(
    Guid StaffUid,
    string Password,
    bool IsHotelAdmin) : IRequest<ResetAdminCredentialsResult>;

public sealed record ResetAdminCredentialsResult
{
    public bool IsSuccessful { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public ResetAdminCredentialsResponse? Data { get; init; }

    public static ResetAdminCredentialsResult Success(ResetAdminCredentialsResponse data) => new()
    {
        IsSuccessful = true,
        Data = data
    };

    public static ResetAdminCredentialsResult Failure(string errorCode, string errorMessage) => new()
    {
        IsSuccessful = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };
}

public sealed record ResetAdminCredentialsResponse(
    Guid StaffUid,
    string Email,
    IReadOnlyList<Guid> PropertyUids);

internal sealed class ResetAdminCredentialsCommandHandler(
    IStaffRepository staffRepository,
    IPasswordHasher<StaffLoginRecord> passwordHasher)
    : IRequestHandler<ResetAdminCredentialsCommand, ResetAdminCredentialsResult>
{
    public async Task<ResetAdminCredentialsResult> Handle(
        ResetAdminCredentialsCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsHotelAdmin)
        {
            return ResetAdminCredentialsResult.Failure(
                "forbidden",
                "Only a property admin can reset these credentials.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return ResetAdminCredentialsResult.Failure(
                "validation_error",
                "Password is required.");
        }

        if (request.Password.Trim().Length < 8)
        {
            return ResetAdminCredentialsResult.Failure(
                "validation_error",
                "Password must be at least 8 characters.");
        }

        var staff = await staffRepository.GetByUidAsync(request.StaffUid, cancellationToken);
        if (staff is null)
        {
            return ResetAdminCredentialsResult.Failure(
                "not_found",
                "Admin account was not found.");
        }

        if (string.IsNullOrWhiteSpace(staff.Email))
        {
            return ResetAdminCredentialsResult.Failure(
                "validation_error",
                "This account has no email. Email cannot be changed; contact SuperAdmin.");
        }

        var passwordHash = passwordHasher.HashPassword(staff, request.Password.Trim());
        await staffRepository.UpdatePasswordAsync(
            request.StaffUid,
            passwordHash,
            cancellationToken);

        return ResetAdminCredentialsResult.Success(
            new ResetAdminCredentialsResponse(
                request.StaffUid,
                staff.Email,
                staff.PropertyUids));
    }
}
