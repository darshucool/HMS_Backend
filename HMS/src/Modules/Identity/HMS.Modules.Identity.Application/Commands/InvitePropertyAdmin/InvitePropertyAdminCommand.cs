using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.Authentication;
using HMS.Modules.Identity.Application.Commands.RegisterStaff;
using HMS.Modules.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HMS.Modules.Identity.Application.Commands.InvitePropertyAdmin;

public sealed record InvitePropertyAdminCommand(
    Guid PropertyUid,
    string Email,
    string FirstName,
    string? LastName,
    Guid? ActorStaffUid,
    string AdminLoginUrl) : IRequest<RegisterStaffResult>;

internal sealed class InvitePropertyAdminCommandHandler(
    IStaffRepository staffRepository,
    IPasswordHasher<StaffLoginRecord> passwordHasher,
    IEmailSender emailSender)
    : IRequestHandler<InvitePropertyAdminCommand, RegisterStaffResult>
{
    public async Task<RegisterStaffResult> Handle(
        InvitePropertyAdminCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PropertyUid == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.FirstName))
        {
            return RegisterStaffResult.Failure(
                "validation_error",
                "Property, email and first name are required.");
        }

        var email = request.Email.Trim();
        if (!email.Contains('@'))
        {
            return RegisterStaffResult.Failure(
                "validation_error",
                "Email is invalid.");
        }

        var property = await staffRepository.GetPropertyByUidAsync(
            request.PropertyUid,
            cancellationToken);

        if (property is null || property.IsArchived)
            return RegisterStaffResult.Failure("not_found", "Property was not found.");

        await staffRepository.EnsureRoleExistsAsync(
            IdentityRoles.HotelAdmin,
            "Hotel Administrator",
            "Full hotel administration access",
            cancellationToken);

        var password = CredentialGenerator.TemporaryPassword();
        var passwordHash = passwordHasher.HashPassword(new StaffLoginRecord(), password);
        var existing = await staffRepository.GetByNormalizedEmailAsync(
            email.ToUpperInvariant(),
            cancellationToken);

        Guid staffUid;
        string fullName;

        try
        {
            if (existing is not null)
            {
                staffUid = existing.Uid;
                fullName = string.Join(
                    " ",
                    new[] { existing.FirstName, existing.LastName }
                        .Where(value => !string.IsNullOrWhiteSpace(value)));

                await staffRepository.AssignPropertyAsync(
                    existing.Uid,
                    request.PropertyUid,
                    IdentityRoles.HotelAdmin,
                    IdentityRoles.ToHotelAccessRole(IdentityRoles.HotelAdmin),
                    request.ActorStaffUid,
                    cancellationToken);

                await staffRepository.UpdatePasswordAsync(
                    existing.Uid,
                    passwordHash,
                    cancellationToken);
            }
            else
            {
                var staff = StaffUser.Create(
                    request.FirstName,
                    request.LastName,
                    email);
                staff.SetPasswordHash(passwordHash);

                await staffRepository.InsertRegisteredUserAsync(
                    staff,
                    request.PropertyUid,
                    IdentityRoles.HotelAdmin,
                    IdentityRoles.ToHotelAccessRole(IdentityRoles.HotelAdmin),
                    request.ActorStaffUid,
                    cancellationToken);

                staffUid = staff.Uid;
                fullName = staff.FullName;
            }
        }
        catch (ArgumentException exception)
        {
            return RegisterStaffResult.Failure("validation_error", exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return RegisterStaffResult.Failure("validation_error", exception.Message);
        }

        var loginUrl = string.IsNullOrWhiteSpace(request.AdminLoginUrl)
            ? "http://localhost:3000/admin/login"
            : request.AdminLoginUrl.Trim();

        var emailResult = await emailSender.SendAsync(
            email,
            $"Access to {property.Name}",
            $"""
            Hello {request.FirstName},

            You have been given admin access to {property.Name}.

            Admin login: {loginUrl}

            Sign in with:
            Email: {email}
            Temporary password: {password}

            Important:
            1. Open the admin login link above.
            2. Sign in with your email and temporary password.
            3. Immediately reset your password using Reset Password / Change Password.
               Your email address cannot be changed. Only your password can be updated.
               Your admin account id stays the same.

            Property id: {request.PropertyUid}
            """,
            cancellationToken);

        return RegisterStaffResult.Success(new RegisterStaffResponse(
            staffUid,
            request.PropertyUid,
            fullName,
            email,
            IdentityRoles.HotelAdmin,
            emailResult.Sent,
            emailResult.Sent ? null : password,
            emailResult.Detail));
    }
}
