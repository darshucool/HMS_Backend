using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using HMS.Modules.Guests.Domain.Enums;
using MediatR;

namespace HMS.Modules.Guests.Application.Commands.UpdateGuest;

public sealed record UpdateGuestCommand(
    Guid GuestUid,
    GuestType GuestType,
    string? Title,
    string? FirstName,
    string? LastName,
    string? DisplayName,
    string? Phone,
    string? AlternatePhone,
    string? Email,
    string? NationalityCode,
    DateOnly? DateOfBirth,
    string? PreferredLanguage,
    string? Address,
    string? City,
    string? CountryCode,
    string? Notes,
    bool IsActive,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<GuestDto>>;

public sealed class UpdateGuestCommandHandler(
    IPropertyAccess propertyAccess,
    IGuestRepository guestRepository)
    : IRequestHandler<UpdateGuestCommand, GuestResult<GuestDto>>
{
    public async Task<GuestResult<GuestDto>> Handle(
        UpdateGuestCommand request,
        CancellationToken cancellationToken)
    {
        var guest = await guestRepository.GetByUidAsync(request.GuestUid, cancellationToken);
        if (guest is null || guest.IsArchived)
            return GuestResult<GuestDto>.NotFound("Guest was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessToOrganizationAsync(
                request.ActorSubject,
                guest.OrganizationId,
                true,
                cancellationToken))
        {
            return GuestResult<GuestDto>.Forbidden("You cannot update this guest.");
        }

        try
        {
            guest.Update(
                request.GuestType,
                request.Title,
                request.FirstName,
                request.LastName,
                request.DisplayName,
                request.Phone,
                request.AlternatePhone,
                request.Email,
                request.NationalityCode,
                request.DateOfBirth,
                request.PreferredLanguage,
                request.Address,
                request.City,
                request.CountryCode,
                request.Notes,
                request.IsActive,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return GuestResult<GuestDto>.Validation(exception.Message);
        }

        await guestRepository.UpdateAsync(guest, cancellationToken);
        return GuestResult<GuestDto>.Success(GuestMapper.ToDto(guest));
    }
}
