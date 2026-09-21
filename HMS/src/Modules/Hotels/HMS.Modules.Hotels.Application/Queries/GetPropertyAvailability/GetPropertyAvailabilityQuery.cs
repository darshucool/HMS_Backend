using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetPropertyAvailability;

public sealed record GetPropertyAvailabilityQuery(
    Guid PropertyUid,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Adults,
    int Children,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<PropertyAvailabilityDto>>;

public sealed class GetPropertyAvailabilityQueryHandler(
    IPropertyRepository propertyRepository,
    IPropertyAvailabilityRepository availabilityRepository)
    : IRequestHandler<GetPropertyAvailabilityQuery, HotelResult<PropertyAvailabilityDto>>
{
    public async Task<HotelResult<PropertyAvailabilityDto>> Handle(
        GetPropertyAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CheckOut <= request.CheckIn)
            return HotelResult<PropertyAvailabilityDto>.Validation("Check-out must be after check-in.");

        if (request.Adults < 1)
            return HotelResult<PropertyAvailabilityDto>.Validation("Adults must be at least 1.");

        if (request.Children < 0)
            return HotelResult<PropertyAvailabilityDto>.Validation("Children cannot be negative.");

        var nights = request.CheckOut.DayNumber - request.CheckIn.DayNumber;
        if (nights > 365)
            return HotelResult<PropertyAvailabilityDto>.Validation("Stay length cannot exceed 365 nights.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<PropertyAvailabilityDto>.Forbidden(
                "You cannot access availability for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<PropertyAvailabilityDto>.NotFound("Property was not found.");

        var items = await availabilityRepository.GetAsync(
            request.PropertyUid,
            request.CheckIn,
            request.CheckOut,
            request.Adults,
            request.Children,
            cancellationToken);

        return HotelResult<PropertyAvailabilityDto>.Success(new PropertyAvailabilityDto(
            property.Uid,
            request.CheckIn,
            request.CheckOut,
            nights,
            request.Adults,
            request.Children,
            property.DefaultCurrency,
            items));
    }
}
