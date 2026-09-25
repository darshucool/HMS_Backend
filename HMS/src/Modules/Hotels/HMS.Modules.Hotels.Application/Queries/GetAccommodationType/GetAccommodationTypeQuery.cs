using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetAccommodationType;

public sealed record GetAccommodationTypeQuery(
    Guid Uid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<AccommodationTypeDto>>;

public sealed class GetAccommodationTypeQueryHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository)
    : IRequestHandler<GetAccommodationTypeQuery, HotelResult<AccommodationTypeDto>>
{
    public async Task<HotelResult<AccommodationTypeDto>> Handle(
        GetAccommodationTypeQuery request,
        CancellationToken cancellationToken)
    {
        var accommodationType = await accommodationTypeRepository.GetByUidAsync(
            request.Uid,
            cancellationToken);

        if (accommodationType is null || accommodationType.IsArchived)
            return HotelResult<AccommodationTypeDto>.NotFound("Accommodation type was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                accommodationType.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<AccommodationTypeDto>.Forbidden(
                "You cannot access this accommodation type.");
        }

        return HotelResult<AccommodationTypeDto>.Success(
            PropertyMapper.ToAccommodationTypeDto(accommodationType));
    }
}
