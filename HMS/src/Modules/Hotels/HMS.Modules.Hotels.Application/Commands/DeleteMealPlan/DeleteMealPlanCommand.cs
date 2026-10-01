using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.DeleteMealPlan;

public sealed record DeleteMealPlanCommand(
    Guid PropertyUid,
    Guid MealPlanUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<bool>>;

public sealed class DeleteMealPlanCommandHandler(
    IPropertyRepository propertyRepository,
    IMealPlanRepository mealPlanRepository)
    : IRequestHandler<DeleteMealPlanCommand, HotelResult<bool>>
{
    public async Task<HotelResult<bool>> Handle(
        DeleteMealPlanCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<bool>.Forbidden("You cannot delete meal plans for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<bool>.NotFound("Property was not found.");

        var mealPlan = await mealPlanRepository.GetByUidAsync(request.MealPlanUid, cancellationToken);
        if (mealPlan is null || mealPlan.IsArchived || mealPlan.PropertyId != property.Id)
            return HotelResult<bool>.NotFound("Meal plan was not found.");

        mealPlan.Archive(request.ActorSubject);
        await mealPlanRepository.ArchiveAsync(mealPlan, cancellationToken);
        return HotelResult<bool>.Success(true);
    }
}
