using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;
using MediatR;

namespace HMS.Modules.Guests.Application.Commands.CreateGuest;

public sealed record CreateGuestCommand(
    Guid PropertyUid,
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
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<GuestDto>>;

public sealed class CreateGuestCommandHandler(
    IPropertyAccess propertyAccess,
    IGuestRepository guestRepository)
    : IRequestHandler<CreateGuestCommand, GuestResult<GuestDto>>
{
    public async Task<GuestResult<GuestDto>> Handle(
        CreateGuestCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return GuestResult<GuestDto>.Forbidden(
                "You cannot create guests for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return GuestResult<GuestDto>.NotFound("Property was not found.");

        Guest guest;
        try
        {
            guest = Guest.Create(
                property.OrganizationId,
                property.OrganizationUid,
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
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return GuestResult<GuestDto>.Validation(exception.Message);
        }

        await guestRepository.InsertAsync(guest, cancellationToken);
        return GuestResult<GuestDto>.Success(GuestMapper.ToDto(guest));
    }
}
