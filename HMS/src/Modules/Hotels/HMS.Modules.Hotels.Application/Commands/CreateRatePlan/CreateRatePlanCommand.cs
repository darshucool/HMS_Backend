using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateRatePlan;

public sealed record CreateRatePlanCommand(
    Guid PropertyUid,
    Guid AccommodationTypeUid,
    Guid? MealPlanUid,
    string Code,
    string Name,
    PricingBasis PricingBasis,
    string Currency,
    string? Description,
    bool IsRefundable,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<RatePlanDto>>;

public sealed class CreateRatePlanCommandHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository,
    IMealPlanRepository mealPlanRepository,
    IRatePlanRepository ratePlanRepository)
    : IRequestHandler<CreateRatePlanCommand, HotelResult<RatePlanDto>>
{
    public async Task<HotelResult<RatePlanDto>> Handle(
        CreateRatePlanCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<RatePlanDto>.Forbidden(
                "You cannot create rate plans for this property.");
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return HotelResult<RatePlanDto>.Validation("Code and name are required.");

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<RatePlanDto>.NotFound("Property was not found.");

        var accommodationType = await accommodationTypeRepository.GetByUidAsync(
            request.AccommodationTypeUid,
            cancellationToken);

        if (accommodationType is null || accommodationType.IsArchived)
            return HotelResult<RatePlanDto>.NotFound("Accommodation type was not found.");

        if (accommodationType.PropertyId != property.Id)
            return HotelResult<RatePlanDto>.Validation(
                "The accommodation type does not belong to this property.");

        MealPlan? mealPlan = null;
        if (request.MealPlanUid is Guid mealPlanUid)
        {
            mealPlan = await mealPlanRepository.GetByUidAsync(mealPlanUid, cancellationToken);
            if (mealPlan is null || mealPlan.IsArchived)
                return HotelResult<RatePlanDto>.NotFound("Meal plan was not found.");

            if (mealPlan.PropertyId != property.Id)
                return HotelResult<RatePlanDto>.Validation(
                    "The meal plan does not belong to this property.");
        }

        if (await ratePlanRepository.CodeExistsAsync(
                property.Id,
                request.Code.Trim().ToUpperInvariant(),
                cancellationToken))
        {
            return HotelResult<RatePlanDto>.Conflict(
                "This rate plan code already exists for the property.");
        }

        RatePlan ratePlan;
        try
        {
            ratePlan = RatePlan.Create(
                property.OrganizationId,
                property.Id,
                property.Uid,
                accommodationType.Id,
                accommodationType.Uid,
                mealPlan?.Id,
                mealPlan?.Uid,
                request.Code,
                request.Name,
                request.PricingBasis,
                request.Currency,
                request.Description,
                request.IsRefundable,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<RatePlanDto>.Validation(exception.Message);
        }

        await ratePlanRepository.InsertAsync(ratePlan, cancellationToken);
        return HotelResult<RatePlanDto>.Success(PropertyMapper.ToRatePlanDto(ratePlan));
    }
}
