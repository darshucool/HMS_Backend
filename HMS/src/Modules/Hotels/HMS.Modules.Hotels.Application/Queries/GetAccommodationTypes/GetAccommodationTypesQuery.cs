using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetAccommodationTypes;

public sealed record GetAccommodationTypesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<IReadOnlyList<AccommodationTypeDto>>>;

public sealed class GetAccommodationTypesQueryHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository)
    : IRequestHandler<GetAccommodationTypesQuery, HotelResult<IReadOnlyList<AccommodationTypeDto>>>
{
    public async Task<HotelResult<IReadOnlyList<AccommodationTypeDto>>> Handle(
        GetAccommodationTypesQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<IReadOnlyList<AccommodationTypeDto>>.Forbidden(
                "You cannot access accommodation types for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<IReadOnlyList<AccommodationTypeDto>>.NotFound("Property was not found.");

        var items = await accommodationTypeRepository.GetByPropertyUidAsync(
            request.PropertyUid,
            cancellationToken);

        return HotelResult<IReadOnlyList<AccommodationTypeDto>>.Success(items);
    }
}
