using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetAccommodationUnit;

public sealed record GetAccommodationUnitQuery(
    Guid Uid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<AccommodationUnitDto>>;

public sealed class GetAccommodationUnitQueryHandler(
    IPropertyRepository propertyRepository,
    IAccommodationUnitRepository accommodationUnitRepository)
    : IRequestHandler<GetAccommodationUnitQuery, HotelResult<AccommodationUnitDto>>
{
    public async Task<HotelResult<AccommodationUnitDto>> Handle(
        GetAccommodationUnitQuery request,
        CancellationToken cancellationToken)
    {
        var unit = await accommodationUnitRepository.GetByUidAsync(request.Uid, cancellationToken);
        if (unit is null || unit.IsArchived)
            return HotelResult<AccommodationUnitDto>.NotFound("Unit was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                unit.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<AccommodationUnitDto>.Forbidden("You cannot access this unit.");
        }

        return HotelResult<AccommodationUnitDto>.Success(PropertyMapper.ToAccommodationUnitDto(unit));
    }
}
