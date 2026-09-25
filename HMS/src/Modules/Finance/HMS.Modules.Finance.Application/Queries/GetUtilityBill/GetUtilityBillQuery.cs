using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetUtilityBill;

public sealed record GetUtilityBillQuery(
    Guid UtilityBillUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<UtilityBillDto>>;

public sealed class GetUtilityBillQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityBillRepository utilityBillRepository)
    : IRequestHandler<GetUtilityBillQuery, FinanceResult<UtilityBillDto>>
{
    public async Task<FinanceResult<UtilityBillDto>> Handle(
        GetUtilityBillQuery request,
        CancellationToken cancellationToken)
    {
        var bill = await utilityBillRepository.GetByUidAsync(request.UtilityBillUid, cancellationToken);
        if (bill is null || bill.IsArchived)
            return FinanceResult<UtilityBillDto>.NotFound("Utility bill was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                bill.PropertyUid,
                false,
                cancellationToken))
        {
            return FinanceResult<UtilityBillDto>.Forbidden("You cannot view this utility bill.");
        }

        var detail = await utilityBillRepository.GetDetailByUidAsync(request.UtilityBillUid, cancellationToken);
        return detail is null
            ? FinanceResult<UtilityBillDto>.NotFound("Utility bill was not found.")
            : FinanceResult<UtilityBillDto>.Success(detail);
    }
}
