using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.DeleteUtilityBill;

public sealed record DeleteUtilityBillCommand(
    Guid UtilityBillUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<bool>>;

public sealed class DeleteUtilityBillCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityBillRepository utilityBillRepository)
    : IRequestHandler<DeleteUtilityBillCommand, FinanceResult<bool>>
{
    public async Task<FinanceResult<bool>> Handle(
        DeleteUtilityBillCommand request,
        CancellationToken cancellationToken)
    {
        var bill = await utilityBillRepository.GetByUidAsync(request.UtilityBillUid, cancellationToken);
        if (bill is null || bill.IsArchived)
            return FinanceResult<bool>.NotFound("Utility bill was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject, bill.PropertyUid, true, cancellationToken))
        {
            return FinanceResult<bool>.Forbidden("You cannot delete this utility bill.");
        }

        await utilityBillRepository.DeleteAsync(bill.Id, request.ActorSubject, cancellationToken);
        return FinanceResult<bool>.Success(true);
    }
}
