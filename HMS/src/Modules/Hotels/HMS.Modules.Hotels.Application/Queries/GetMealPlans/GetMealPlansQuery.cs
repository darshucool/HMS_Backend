using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetMealPlans;

public sealed record GetMealPlansQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<IReadOnlyList<MealPlanDto>>>;

public sealed class GetMealPlansQueryHandler(
    IPropertyRepository propertyRepository,
    IMealPlanRepository mealPlanRepository)
    : IRequestHandler<GetMealPlansQuery, HotelResult<IReadOnlyList<MealPlanDto>>>
{
    public async Task<HotelResult<IReadOnlyList<MealPlanDto>>> Handle(
        GetMealPlansQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<IReadOnlyList<MealPlanDto>>.Forbidden(
                "You cannot access meal plans for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<IReadOnlyList<MealPlanDto>>.NotFound("Property was not found.");

        var items = await mealPlanRepository.GetByPropertyUidAsync(
            request.PropertyUid,
            cancellationToken);

        return HotelResult<IReadOnlyList<MealPlanDto>>.Success(items);
    }
}
