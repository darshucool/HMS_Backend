using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Application.Queries.GetUtilityTypes;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetUtilityType;

public sealed record GetUtilityTypeQuery(
    Guid PropertyUid,
    Guid UtilityTypeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<UtilityTypeDto>>;

public sealed class GetUtilityTypeQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityTypeRepository utilityTypeRepository)
    : IRequestHandler<GetUtilityTypeQuery, FinanceResult<UtilityTypeDto>>
{
    public async Task<FinanceResult<UtilityTypeDto>> Handle(
        GetUtilityTypeQuery request,
        CancellationToken cancellationToken)
    {
        var access = await GetUtilityTypesQueryHandler.Authorize<UtilityTypeDto>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, false, cancellationToken);
        if (access.Error is not null)
            return access.Error;

        var detail = await utilityTypeRepository.GetDetailAsync(
            request.UtilityTypeUid, access.OrganizationId, cancellationToken);
        return detail is null
            ? FinanceResult<UtilityTypeDto>.NotFound("Utility type was not found.")
            : FinanceResult<UtilityTypeDto>.Success(detail);
    }
}
