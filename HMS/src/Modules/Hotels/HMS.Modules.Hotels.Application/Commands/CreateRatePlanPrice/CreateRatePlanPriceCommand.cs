using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateRatePlanPrice;

public sealed record CreateRatePlanPriceCommand(
    Guid RatePlanUid,
    DateOnly StartDate,
    DateOnly EndDate,
    short? DayOfWeek,
    decimal? AdultRate,
    decimal? ChildRate,
    decimal UnitRate,
    int MinimumStay,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<RatePlanPriceDto>>;

public sealed class CreateRatePlanPriceCommandHandler(
    IPropertyRepository propertyRepository,
    IRatePlanRepository ratePlanRepository,
    IRatePlanPriceRepository ratePlanPriceRepository)
    : IRequestHandler<CreateRatePlanPriceCommand, HotelResult<RatePlanPriceDto>>
{
    public async Task<HotelResult<RatePlanPriceDto>> Handle(
        CreateRatePlanPriceCommand request,
        CancellationToken cancellationToken)
    {
        var ratePlan = await ratePlanRepository.GetByUidAsync(request.RatePlanUid, cancellationToken);
        if (ratePlan is null || ratePlan.IsArchived)
            return HotelResult<RatePlanPriceDto>.NotFound("Rate plan was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                ratePlan.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<RatePlanPriceDto>.Forbidden(
                "You cannot create prices for this rate plan.");
        }

        RatePlanPrice price;
        try
        {
            price = RatePlanPrice.Create(
                ratePlan.OrganizationId,
                ratePlan.PropertyId,
                ratePlan.PropertyUid,
                ratePlan.Id,
                ratePlan.Uid,
                request.StartDate,
                request.EndDate,
                request.DayOfWeek,
                request.AdultRate,
                request.ChildRate,
                request.UnitRate,
                request.MinimumStay,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<RatePlanPriceDto>.Validation(exception.Message);
        }

        await ratePlanPriceRepository.InsertAsync(price, cancellationToken);
        return HotelResult<RatePlanPriceDto>.Success(PropertyMapper.ToRatePlanPriceDto(price));
    }
}
