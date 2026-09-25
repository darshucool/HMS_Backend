using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using MediatR;

namespace HMS.Modules.Guests.Application.Queries.GetGuest;

public sealed record GetGuestQuery(
    Guid GuestUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<GuestDto>>;

public sealed class GetGuestQueryHandler(
    IPropertyAccess propertyAccess,
    IGuestRepository guestRepository)
    : IRequestHandler<GetGuestQuery, GuestResult<GuestDto>>
{
    public async Task<GuestResult<GuestDto>> Handle(
        GetGuestQuery request,
        CancellationToken cancellationToken)
    {
        var guest = await guestRepository.GetByUidAsync(request.GuestUid, cancellationToken);
        if (guest is null || guest.IsArchived)
            return GuestResult<GuestDto>.NotFound("Guest was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessToOrganizationAsync(
                request.ActorSubject,
                guest.OrganizationId,
                false,
                cancellationToken))
        {
            return GuestResult<GuestDto>.Forbidden("You cannot access this guest.");
        }

        return GuestResult<GuestDto>.Success(GuestMapper.ToDto(guest));
    }
}
