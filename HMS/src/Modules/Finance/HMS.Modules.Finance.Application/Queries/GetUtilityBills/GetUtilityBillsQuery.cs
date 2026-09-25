using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetUtilityBills;

public sealed record GetUtilityBillsQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IReadOnlyList<UtilityBillDto>>>;

public sealed class GetUtilityBillsQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityBillRepository utilityBillRepository)
    : IRequestHandler<GetUtilityBillsQuery, FinanceResult<IReadOnlyList<UtilityBillDto>>>
{
    public async Task<FinanceResult<IReadOnlyList<UtilityBillDto>>> Handle(
        GetUtilityBillsQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return FinanceResult<IReadOnlyList<UtilityBillDto>>.Forbidden(
                "You cannot view utility bills for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<IReadOnlyList<UtilityBillDto>>.NotFound("Property was not found.");

        var items = await utilityBillRepository.GetByPropertyUidAsync(request.PropertyUid, cancellationToken);
        return FinanceResult<IReadOnlyList<UtilityBillDto>>.Success(items);
    }
}
