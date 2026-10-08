using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetOtherIncomeByUid;

public sealed record GetOtherIncomeByUidQuery(
    Guid PropertyUid,
    Guid OtherIncomeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<OtherIncomeDto>>;

public sealed class GetOtherIncomeByUidQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IOtherIncomeRepository otherIncomeRepository)
    : IRequestHandler<GetOtherIncomeByUidQuery, FinanceResult<OtherIncomeDto>>
{
    public async Task<FinanceResult<OtherIncomeDto>> Handle(
        GetOtherIncomeByUidQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return FinanceResult<OtherIncomeDto>.Forbidden(
                "You cannot view other income for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<OtherIncomeDto>.NotFound("Property was not found.");

        var item = await otherIncomeRepository.GetByUidAsync(
            request.PropertyUid,
            request.OtherIncomeUid,
            cancellationToken);

        return item is null
            ? FinanceResult<OtherIncomeDto>.NotFound("Other income was not found.")
            : FinanceResult<OtherIncomeDto>.Success(item);
    }
}
