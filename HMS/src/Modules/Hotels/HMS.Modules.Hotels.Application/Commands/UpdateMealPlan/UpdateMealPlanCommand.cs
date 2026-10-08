using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.UpdateMealPlan;

public sealed record UpdateMealPlanCommand(
    Guid PropertyUid,
    Guid MealPlanUid,
    string Code,
    string Name,
    string? Description,
    bool IncludesBreakfast,
    bool IncludesLunch,
    bool IncludesDinner,
    bool AllowByo,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<MealPlanDto>>;

public sealed class UpdateMealPlanCommandHandler(
    IPropertyRepository propertyRepository,
    IMealPlanRepository mealPlanRepository)
    : IRequestHandler<UpdateMealPlanCommand, HotelResult<MealPlanDto>>
{
    public async Task<HotelResult<MealPlanDto>> Handle(
        UpdateMealPlanCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<MealPlanDto>.Forbidden("You cannot update meal plans for this property.");
        }

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<MealPlanDto>.NotFound("Property was not found.");

        var mealPlan = await mealPlanRepository.GetByUidAsync(request.MealPlanUid, cancellationToken);
        if (mealPlan is null || mealPlan.IsArchived || mealPlan.PropertyId != property.Id)
            return HotelResult<MealPlanDto>.NotFound("Meal plan was not found.");

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return HotelResult<MealPlanDto>.Validation("Code and name are required.");

        if (await mealPlanRepository.CodeExistsAsync(
                property.Id,
                request.Code.Trim().ToUpperInvariant(),
                mealPlan.Uid,
                cancellationToken))
        {
            return HotelResult<MealPlanDto>.Conflict(
                "This meal plan code already exists for the property.");
        }

        try
        {
            mealPlan.Update(
                request.Code,
                request.Name,
                request.Description,
                request.IncludesBreakfast,
                request.IncludesLunch,
                request.IncludesDinner,
                request.AllowByo,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<MealPlanDto>.Validation(exception.Message);
        }

        await mealPlanRepository.UpdateAsync(mealPlan, cancellationToken);
        return HotelResult<MealPlanDto>.Success(PropertyMapper.ToMealPlanDto(mealPlan));
    }
}
