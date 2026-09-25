using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.Authentication;
using HMS.Modules.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace HMS.Modules.Identity.Application.Commands.RegisterStaff;

internal sealed class RegisterStaffCommandHandler(
    IStaffRepository staffRepository,
    IPasswordHasher<StaffLoginRecord> passwordHasher)
    : IRequestHandler<RegisterStaffCommand, RegisterStaffResult>
{
    public async Task<RegisterStaffResult> Handle(
        RegisterStaffCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin && !request.IsHotelAdmin)
        {
            return RegisterStaffResult.Failure(
                "forbidden",
                "You cannot register staff for this property.");
        }

        if (!request.IsPlatformAdmin &&
            request.ActorPropertyUid is Guid actorProperty &&
            actorProperty != request.PropertyUid)
        {
            return RegisterStaffResult.Failure(
                "forbidden",
                "You can only register staff for your own property.");
        }

        if (request.PropertyUid == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName))
        {
            return RegisterStaffResult.Failure(
                "validation_error",
                "Property, email, password and first name are required.");
        }

        if (request.Password.Trim().Length < 8)
        {
            return RegisterStaffResult.Failure(
                "validation_error",
                "Password must be at least 8 characters.");
        }

        var roleCode = string.IsNullOrWhiteSpace(request.RoleCode)
            ? IdentityRoles.FrontDesk
            : request.RoleCode.Trim().ToUpperInvariant();

        if (!IdentityRoles.StaffRegisterRoles.Contains(roleCode, StringComparer.OrdinalIgnoreCase) &&
            !string.Equals(roleCode, IdentityRoles.HotelAdmin, StringComparison.OrdinalIgnoreCase))
        {
            return RegisterStaffResult.Failure(
                "validation_error",
                "The selected role is not allowed.");
        }

        var isAdminRole = string.Equals(
            roleCode,
            IdentityRoles.HotelAdmin,
            StringComparison.OrdinalIgnoreCase);

        if (isAdminRole && !request.IsPlatformAdmin)
        {
            return RegisterStaffResult.Failure(
                "forbidden",
                "Only the platform super admin can register hotel admins.");
        }

        var property = await staffRepository.GetPropertyByUidAsync(
            request.PropertyUid,
            cancellationToken);

        if (property is null || property.IsArchived)
            return RegisterStaffResult.Failure("not_found", "Property was not found.");

        await staffRepository.EnsureRoleExistsAsync(
            roleCode,
            roleCode.Replace('_', ' '),
            null,
            cancellationToken);

        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await staffRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return RegisterStaffResult.Failure(
                "conflict",
                "This email is already taken.");
        }

        StaffUser staff;
        try
        {
            staff = StaffUser.Create(
                request.FirstName,
                request.LastName,
                request.Email);
        }
        catch (ArgumentException exception)
        {
            return RegisterStaffResult.Failure("validation_error", exception.Message);
        }

        staff.SetPasswordHash(
            passwordHasher.HashPassword(new StaffLoginRecord(), request.Password));

        try
        {
            await staffRepository.InsertRegisteredUserAsync(
                staff,
                request.PropertyUid,
                roleCode,
                IdentityRoles.ToHotelAccessRole(roleCode),
                request.ActorStaffUid,
                cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return RegisterStaffResult.Failure("validation_error", exception.Message);
        }

        return RegisterStaffResult.Success(new RegisterStaffResponse(
            staff.Uid,
            request.PropertyUid,
            staff.FullName,
            staff.Email,
            roleCode));
    }
}
