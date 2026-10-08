using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetUtilityTypes;

public sealed record GetUtilityTypesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IReadOnlyList<UtilityTypeDto>>>;

public sealed class GetUtilityTypesQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityTypeRepository utilityTypeRepository)
    : IRequestHandler<GetUtilityTypesQuery, FinanceResult<IReadOnlyList<UtilityTypeDto>>>
{
    public async Task<FinanceResult<IReadOnlyList<UtilityTypeDto>>> Handle(
        GetUtilityTypesQuery request,
        CancellationToken cancellationToken)
    {
        var access = await Authorize<IReadOnlyList<UtilityTypeDto>>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, false, cancellationToken);
        if (access.Error is not null)
            return access.Error;

        var items = await utilityTypeRepository.GetByOrganizationIdAsync(access.OrganizationId, cancellationToken);
        return FinanceResult<IReadOnlyList<UtilityTypeDto>>.Success(items);
    }

    internal static async Task<(FinanceResult<T>? Error, long OrganizationId)> Authorize<T>(
        IFinancePropertyAccess propertyAccess,
        Guid propertyUid,
        string actorSubject,
        bool isPlatformAdmin,
        bool requireManager,
        CancellationToken cancellationToken)
    {
        if (!isPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(actorSubject, propertyUid, requireManager, cancellationToken))
        {
            return (FinanceResult<T>.Forbidden("You cannot access utility types for this property."), 0);
        }

        var property = await propertyAccess.GetByUidAsync(propertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return (FinanceResult<T>.NotFound("Property was not found."), 0);

        return (null, property.OrganizationId);
    }
}
