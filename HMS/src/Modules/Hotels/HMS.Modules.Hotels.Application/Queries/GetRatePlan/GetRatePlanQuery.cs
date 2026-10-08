using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetRatePlan;

public sealed record GetRatePlanQuery(
    Guid RatePlanUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<RatePlanDto>>;

public sealed class GetRatePlanQueryHandler(
    IPropertyRepository propertyRepository,
    IRatePlanRepository ratePlanRepository)
    : IRequestHandler<GetRatePlanQuery, HotelResult<RatePlanDto>>
{
    public async Task<HotelResult<RatePlanDto>> Handle(
        GetRatePlanQuery request,
        CancellationToken cancellationToken)
    {
        var ratePlan = await ratePlanRepository.GetByUidAsync(request.RatePlanUid, cancellationToken);
        if (ratePlan is null || ratePlan.IsArchived)
            return HotelResult<RatePlanDto>.NotFound("Rate plan was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                ratePlan.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<RatePlanDto>.Forbidden("You cannot access this rate plan.");
        }

        return HotelResult<RatePlanDto>.Success(PropertyMapper.ToRatePlanDto(ratePlan));
    }
}
