using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.UpdateRatePlan;

public sealed record UpdateRatePlanCommand(
    Guid RatePlanUid,
    string Code,
    string Name,
    PricingBasis PricingBasis,
    string Currency,
    string? Description,
    bool IsRefundable,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<RatePlanDto>>;

public sealed class UpdateRatePlanCommandHandler(
    IPropertyRepository propertyRepository,
    IRatePlanRepository ratePlanRepository)
    : IRequestHandler<UpdateRatePlanCommand, HotelResult<RatePlanDto>>
{
    public async Task<HotelResult<RatePlanDto>> Handle(
        UpdateRatePlanCommand request,
        CancellationToken cancellationToken)
    {
        var ratePlan = await ratePlanRepository.GetByUidAsync(request.RatePlanUid, cancellationToken);
        if (ratePlan is null || ratePlan.IsArchived)
            return HotelResult<RatePlanDto>.NotFound("Rate plan was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                ratePlan.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<RatePlanDto>.Forbidden("You cannot update this rate plan.");
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return HotelResult<RatePlanDto>.Validation("Code and name are required.");

        if (await ratePlanRepository.CodeExistsAsync(
                ratePlan.PropertyId,
                request.Code.Trim().ToUpperInvariant(),
                ratePlan.Uid,
                cancellationToken))
        {
            return HotelResult<RatePlanDto>.Conflict(
                "This rate plan code already exists for the property.");
        }

        try
        {
            ratePlan.Update(
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

        await ratePlanRepository.UpdateAsync(ratePlan, cancellationToken);
        return HotelResult<RatePlanDto>.Success(PropertyMapper.ToRatePlanDto(ratePlan));
    }
}
