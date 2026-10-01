using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.DeleteRatePlan;

public sealed record DeleteRatePlanCommand(
    Guid RatePlanUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<bool>>;

public sealed class DeleteRatePlanCommandHandler(
    IPropertyRepository propertyRepository,
    IRatePlanRepository ratePlanRepository)
    : IRequestHandler<DeleteRatePlanCommand, HotelResult<bool>>
{
    public async Task<HotelResult<bool>> Handle(
        DeleteRatePlanCommand request,
        CancellationToken cancellationToken)
    {
        var ratePlan = await ratePlanRepository.GetByUidAsync(request.RatePlanUid, cancellationToken);
        if (ratePlan is null || ratePlan.IsArchived)
            return HotelResult<bool>.NotFound("Rate plan was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                ratePlan.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<bool>.Forbidden("You cannot delete this rate plan.");
        }

        ratePlan.Archive(request.ActorSubject);
        await ratePlanRepository.ArchiveAsync(ratePlan, cancellationToken);
        return HotelResult<bool>.Success(true);
    }
}
