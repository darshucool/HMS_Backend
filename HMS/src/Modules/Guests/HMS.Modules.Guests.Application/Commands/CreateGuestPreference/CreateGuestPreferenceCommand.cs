using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;
using MediatR;

namespace HMS.Modules.Guests.Application.Commands.CreateGuestPreference;

public sealed record CreateGuestPreferenceCommand(
    Guid GuestUid,
    PreferenceType PreferenceType,
    string PreferenceKey,
    string PreferenceValue,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<GuestPreferenceDto>>;

public sealed class CreateGuestPreferenceCommandHandler(
    IPropertyAccess propertyAccess,
    IGuestRepository guestRepository,
    IGuestPreferenceRepository preferenceRepository)
    : IRequestHandler<CreateGuestPreferenceCommand, GuestResult<GuestPreferenceDto>>
{
    public async Task<GuestResult<GuestPreferenceDto>> Handle(
        CreateGuestPreferenceCommand request,
        CancellationToken cancellationToken)
    {
        var guest = await guestRepository.GetByUidAsync(request.GuestUid, cancellationToken);
        if (guest is null || guest.IsArchived)
            return GuestResult<GuestPreferenceDto>.NotFound("Guest was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessToOrganizationAsync(
                request.ActorSubject,
                guest.OrganizationId,
                true,
                cancellationToken))
        {
            return GuestResult<GuestPreferenceDto>.Forbidden(
                "You cannot add preferences for this guest.");
        }

        GuestPreference preference;
        try
        {
            preference = GuestPreference.Create(
                guest.OrganizationId,
                guest.Id,
                guest.Uid,
                request.PreferenceType,
                request.PreferenceKey,
                request.PreferenceValue,
                request.Notes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return GuestResult<GuestPreferenceDto>.Validation(exception.Message);
        }

        await preferenceRepository.InsertAsync(preference, cancellationToken);
        return GuestResult<GuestPreferenceDto>.Success(GuestMapper.ToDto(preference));
    }
}
