using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.Common;
using HMS.Modules.Guests.Application.DTOs;
using MediatR;

namespace HMS.Modules.Guests.Application.Queries.SearchGuests;

public sealed record SearchGuestsQuery(
    string? Phone,
    string? Name,
    string? Email,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<GuestResult<IReadOnlyList<GuestDto>>>;

public sealed class SearchGuestsQueryHandler(IGuestRepository guestRepository)
    : IRequestHandler<SearchGuestsQuery, GuestResult<IReadOnlyList<GuestDto>>>
{
    public async Task<GuestResult<IReadOnlyList<GuestDto>>> Handle(
        SearchGuestsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Phone)
            && string.IsNullOrWhiteSpace(request.Name)
            && string.IsNullOrWhiteSpace(request.Email))
        {
            return GuestResult<IReadOnlyList<GuestDto>>.Validation(
                "Provide at least one of phone, name, or email.");
        }

        var items = await guestRepository.SearchAsync(
            request.ActorSubject,
            request.IsPlatformAdmin,
            request.Phone,
            request.Name,
            request.Email,
            cancellationToken);

        return GuestResult<IReadOnlyList<GuestDto>>.Success(items);
    }
}
