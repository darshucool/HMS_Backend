using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using MediatR;

namespace HMS.Modules.Guests.Application.Queries.GetGuests;

public sealed record GetGuestsQuery(
    Guid PropertyUid,
    string? Search,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<IReadOnlyList<GuestDto>>>;

public sealed class GetGuestsQueryHandler(
    IPropertyAccess propertyAccess,
    IGuestRepository guestRepository)
    : IRequestHandler<GetGuestsQuery, GuestResult<IReadOnlyList<GuestDto>>>
{
    public async Task<GuestResult<IReadOnlyList<GuestDto>>> Handle(
        GetGuestsQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return GuestResult<IReadOnlyList<GuestDto>>.Forbidden(
                "You cannot access guests for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return GuestResult<IReadOnlyList<GuestDto>>.NotFound("Property was not found.");

        var items = await guestRepository.GetByPropertyUidAsync(
            request.PropertyUid,
            request.Search,
            cancellationToken);

        return GuestResult<IReadOnlyList<GuestDto>>.Success(items);
    }
}
