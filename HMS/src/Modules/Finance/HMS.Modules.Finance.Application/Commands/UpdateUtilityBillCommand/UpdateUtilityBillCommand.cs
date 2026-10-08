using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.UpdateUtilityBill;

public sealed record UpdateUtilityBillCommand(
    Guid UtilityBillUid,
    Guid UtilityTypeUid,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal? PreviousReading,
    decimal? CurrentReading,
    decimal? UnitsUsed,
    decimal Amount,
    string Currency,
    DateOnly? DueDate,
    DateTimeOffset? PaidAt,
    string? ReferenceNumber,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<UtilityBillDto>>;

public sealed class UpdateUtilityBillCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityBillRepository utilityBillRepository)
    : IRequestHandler<UpdateUtilityBillCommand, FinanceResult<UtilityBillDto>>
{
    public async Task<FinanceResult<UtilityBillDto>> Handle(
        UpdateUtilityBillCommand request,
        CancellationToken cancellationToken)
    {
        var bill = await utilityBillRepository.GetByUidAsync(request.UtilityBillUid, cancellationToken);
        if (bill is null || bill.IsArchived)
            return FinanceResult<UtilityBillDto>.NotFound("Utility bill was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject, bill.PropertyUid, true, cancellationToken))
        {
            return FinanceResult<UtilityBillDto>.Forbidden("You cannot modify this utility bill.");
        }

        var utilityType = await utilityBillRepository.GetTypeAsync(request.UtilityTypeUid, cancellationToken);
        if (utilityType is null || utilityType.OrganizationId != bill.OrganizationId)
            return FinanceResult<UtilityBillDto>.Validation("Utility type was not found.");

        try
        {
            bill.Update(
                utilityType.Id, request.UtilityTypeUid,
                request.PeriodStart, request.PeriodEnd,
                request.PreviousReading, request.CurrentReading, request.UnitsUsed,
                request.Amount, request.Currency, request.DueDate, request.PaidAt,
                request.ReferenceNumber, request.Notes, request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return FinanceResult<UtilityBillDto>.Validation(exception.Message);
        }

        await utilityBillRepository.UpdateAsync(bill, cancellationToken);

        var detail = await utilityBillRepository.GetDetailByUidAsync(request.UtilityBillUid, cancellationToken);
        return detail is null
            ? FinanceResult<UtilityBillDto>.NotFound("Utility bill was not found.")
            : FinanceResult<UtilityBillDto>.Success(detail);
    }
}