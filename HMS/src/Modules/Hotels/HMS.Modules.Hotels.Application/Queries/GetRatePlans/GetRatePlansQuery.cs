using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetRatePlans;

public sealed record GetRatePlansQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<IReadOnlyList<RatePlanDto>>>;

public sealed class GetRatePlansQueryHandler(
    IPropertyRepository propertyRepository,
    IRatePlanRepository ratePlanRepository)
    : IRequestHandler<GetRatePlansQuery, HotelResult<IReadOnlyList<RatePlanDto>>>
{
    public async Task<HotelResult<IReadOnlyList<RatePlanDto>>> Handle(
        GetRatePlansQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<IReadOnlyList<RatePlanDto>>.Forbidden(
                "You cannot access rate plans for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<IReadOnlyList<RatePlanDto>>.NotFound("Property was not found.");

        var items = await ratePlanRepository.GetByPropertyUidAsync(
            request.PropertyUid,
            cancellationToken);

        return HotelResult<IReadOnlyList<RatePlanDto>>.Success(items);
    }
}
