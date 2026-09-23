using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using MediatR;

namespace HMS.Modules.Guests.Application.Queries.GetGuestBookingHistory;

public sealed record GetGuestBookingHistoryQuery(
    Guid GuestUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<IReadOnlyList<GuestBookingHistoryDto>>>;

public sealed class GetGuestBookingHistoryQueryHandler(
    IPropertyAccess propertyAccess,
    IGuestRepository guestRepository)
    : IRequestHandler<GetGuestBookingHistoryQuery, GuestResult<IReadOnlyList<GuestBookingHistoryDto>>>
{
    public async Task<GuestResult<IReadOnlyList<GuestBookingHistoryDto>>> Handle(
        GetGuestBookingHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var guest = await guestRepository.GetByUidAsync(request.GuestUid, cancellationToken);
        if (guest is null || guest.IsArchived)
            return GuestResult<IReadOnlyList<GuestBookingHistoryDto>>.NotFound("Guest was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessToOrganizationAsync(
                request.ActorSubject,
                guest.OrganizationId,
                false,
                cancellationToken))
        {
            return GuestResult<IReadOnlyList<GuestBookingHistoryDto>>.Forbidden(
                "You cannot access this guest.");
        }

        var items = await guestRepository.GetBookingHistoryAsync(request.GuestUid, cancellationToken);
        return GuestResult<IReadOnlyList<GuestBookingHistoryDto>>.Success(items);
    }
}
