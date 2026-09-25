using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateMealPlan;

public sealed record CreateMealPlanCommand(
    Guid PropertyUid,
    string Code,
    string Name,
    string? Description,
    bool IncludesBreakfast,
    bool IncludesLunch,
    bool IncludesDinner,
    bool AllowByo,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<MealPlanDto>>;

public sealed class CreateMealPlanCommandHandler(
    IPropertyRepository propertyRepository,
    IMealPlanRepository mealPlanRepository)
    : IRequestHandler<CreateMealPlanCommand, HotelResult<MealPlanDto>>
{
    public async Task<HotelResult<MealPlanDto>> Handle(
        CreateMealPlanCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<MealPlanDto>.Forbidden(
                "You cannot create meal plans for this property.");
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return HotelResult<MealPlanDto>.Validation("Code and name are required.");

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<MealPlanDto>.NotFound("Property was not found.");

        if (await mealPlanRepository.CodeExistsAsync(
                property.Id,
                request.Code.Trim().ToUpperInvariant(),
                cancellationToken))
        {
            return HotelResult<MealPlanDto>.Conflict(
                "This meal plan code already exists for the property.");
        }

        MealPlan mealPlan;
        try
        {
            mealPlan = MealPlan.Create(
                property.OrganizationId,
                property.Id,
                property.Uid,
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

        await mealPlanRepository.InsertAsync(mealPlan, cancellationToken);
        return HotelResult<MealPlanDto>.Success(PropertyMapper.ToMealPlanDto(mealPlan));
    }
}
