using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetOtherIncome;

public sealed record GetOtherIncomeQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IReadOnlyList<OtherIncomeDto>>>;

public sealed class GetOtherIncomeQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IOtherIncomeRepository otherIncomeRepository)
    : IRequestHandler<GetOtherIncomeQuery, FinanceResult<IReadOnlyList<OtherIncomeDto>>>
{
    public async Task<FinanceResult<IReadOnlyList<OtherIncomeDto>>> Handle(
        GetOtherIncomeQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return FinanceResult<IReadOnlyList<OtherIncomeDto>>.Forbidden(
                "You cannot view other income for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<IReadOnlyList<OtherIncomeDto>>.NotFound("Property was not found.");

        var items = await otherIncomeRepository.GetByPropertyUidAsync(request.PropertyUid, cancellationToken);
        return FinanceResult<IReadOnlyList<OtherIncomeDto>>.Success(items);
    }
}
