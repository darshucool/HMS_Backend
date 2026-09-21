using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetAccommodationUnits;

public sealed record GetAccommodationUnitsQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<IReadOnlyList<AccommodationUnitDto>>>;

public sealed class GetAccommodationUnitsQueryHandler(
    IPropertyRepository propertyRepository,
    IAccommodationUnitRepository accommodationUnitRepository)
    : IRequestHandler<GetAccommodationUnitsQuery, HotelResult<IReadOnlyList<AccommodationUnitDto>>>
{
    public async Task<HotelResult<IReadOnlyList<AccommodationUnitDto>>> Handle(
        GetAccommodationUnitsQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<IReadOnlyList<AccommodationUnitDto>>.Forbidden(
                "You cannot access units for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<IReadOnlyList<AccommodationUnitDto>>.NotFound("Property was not found.");

        var items = await accommodationUnitRepository.GetByPropertyUidAsync(
            request.PropertyUid,
            cancellationToken);

        return HotelResult<IReadOnlyList<AccommodationUnitDto>>.Success(items);
    }
}
