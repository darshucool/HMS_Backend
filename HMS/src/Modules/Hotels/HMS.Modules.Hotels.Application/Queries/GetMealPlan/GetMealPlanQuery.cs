using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Queries.GetMealPlan;

public sealed record GetMealPlanQuery(
    Guid PropertyUid,
    Guid MealPlanUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<MealPlanDto>>;

public sealed class GetMealPlanQueryHandler(
    IPropertyRepository propertyRepository,
    IMealPlanRepository mealPlanRepository)
    : IRequestHandler<GetMealPlanQuery, HotelResult<MealPlanDto>>
{
    public async Task<HotelResult<MealPlanDto>> Handle(
        GetMealPlanQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return HotelResult<MealPlanDto>.Forbidden("You cannot access meal plans for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<MealPlanDto>.NotFound("Property was not found.");

        var mealPlan = await mealPlanRepository.GetByUidAsync(request.MealPlanUid, cancellationToken);
        if (mealPlan is null || mealPlan.IsArchived || mealPlan.PropertyUid != request.PropertyUid)
            return HotelResult<MealPlanDto>.NotFound("Meal plan was not found.");

        return HotelResult<MealPlanDto>.Success(PropertyMapper.ToMealPlanDto(mealPlan));
    }
}
